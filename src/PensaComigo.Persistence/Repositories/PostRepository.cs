using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class PostRepository(PensaComigoDbContext db) : IPostRepository
{
    public Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.AnyAsync(p => p.Id == id, ct);

    /// <summary>Whitelist do feed. `tag` filtra pela coleção (vira EXISTS na junção);
    /// nada fora daqui é filtrável, por mais que o cliente peça.</summary>
    private static readonly IGridifyMapper<Post> Mapper = new GridifyMapper<Post>()
        // unaccent() na coluna + termo sem acento no C#: "oracao" acha "oração" (issue #30).
        .AddMap("titulo", p => EF.Functions.Unaccent(p.Titulo), v => Textos.RemoverAcentos(v))
        .AddMap("slug", p => p.Slug)
        .AddMap("autor", p => p.Autor.Nome)
        .AddMap("tag", p => p.Tags.Select(t => t.Slug))
        .AddMap("mood", p => p.Moods.Select(m => m), v => Enum.Parse<Domain.Enums.Mood>(v, true))
        .AddMap("etapa", p => (int?)p.Etapa!.Numero)
        .AddMap("dataCriacao", p => p.DataCriacao)
        .AddMap("dataPublicacao", p => p.DataPublicacao)
        .AddMap("status", p => p.Status, v => Enum.Parse<Domain.Enums.StatusPost>(v, true));

    public async Task<Pagina<Post>> ListarAsync(IGridifyQuery consulta, bool incluirRascunhos = false, CancellationToken ct = default)
    {
        // Feed ordena pela data de PUBLICAÇÃO: rascunho antigo publicado hoje sobe pro topo.
        // ponytail: DESC no Postgres põe NULL (rascunho) primeiro na listagem do autor — aceitável.
        if (string.IsNullOrWhiteSpace(consulta.OrderBy)) consulta.OrderBy = "dataPublicacao desc";

        // Público vê publicado E agendado cuja hora já chegou — publicação resolvida na
        // consulta, sem job de fundo (issue #29).
        var agora = DateTime.UtcNow;
        var fonte = incluirRascunhos
            ? db.Posts
            : db.Posts.Where(p => p.Status == Domain.Enums.StatusPost.Publicado ||
                                  (p.Status == Domain.Enums.StatusPost.Agendado && p.DataPublicacao <= agora));

        var (total, query) = await fonte.AsNoTracking().GridifyQueryableAsync(consulta, Mapper, ct);

        // Projeção explícita (sem o Conteudo jsonb — issue #19): 20 posts por página com o corpo
        // junto seria payload absurdo. EF não projeta direto em entidade, daí o shape anônimo
        // no SQL e a remontagem do Post em memória.
        var linhas = await query.Select(p => new
        {
            p.Id, p.Titulo, p.Dek, p.Slug, p.ImagemCapa, p.TempoLeitura,
            p.QtdCurtidas, p.QtdVisualizacoes, p.DataCriacao, p.Status, p.DataPublicacao,
            p.Moods, p.Etapa,
            Autor = new { p.Autor.Id, p.Autor.Nome, p.Autor.ImagemUrl },
            Tags = p.Tags.Select(t => new { t.Id, t.Nome, t.Slug }).ToList(),
        }).ToListAsync(ct);

        var itens = linhas.Select(l => new Post
        {
            Id = l.Id, Titulo = l.Titulo, Dek = l.Dek, Slug = l.Slug, ImagemCapa = l.ImagemCapa,
            TempoLeitura = l.TempoLeitura, QtdCurtidas = l.QtdCurtidas,
            QtdVisualizacoes = l.QtdVisualizacoes, DataCriacao = l.DataCriacao,
            Status = l.Status, DataPublicacao = l.DataPublicacao,
            Moods = l.Moods, Etapa = l.Etapa,
            Autor = new Usuario { Id = l.Autor.Id, Nome = l.Autor.Nome, ImagemUrl = l.Autor.ImagemUrl },
            Tags = l.Tags.Select(t => new Tag { Id = t.Id, Nome = t.Nome, Slug = t.Slug }).ToList(),
        }).ToList();

        return new Pagina<Post>(itens, total);
    }

    public Task<Post?> ObterPorSlugAsync(string slug, CancellationToken ct = default) =>
        db.Posts.AsNoTracking()
                .Include(p => p.Autor)
                .Include(p => p.Tags)
                .Include(p => p.Etapa)
                .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public Task<Post?> ObterDetalhePorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.AsNoTracking()
                .Include(p => p.Autor)
                .Include(p => p.Tags)
                .Include(p => p.Etapa)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

    // UPDATE ... SET qtd_visualizacoes = qtd_visualizacoes + 1 direto no banco.
    // Ler-somar-gravar pelo change tracker perderia contagem com dois leitores simultâneos.
    public Task IncrementarVisualizacoesAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.QtdVisualizacoes, p => p.QtdVisualizacoes + 1), ct);

    // Mesma ideia do contador de visualizações, agora nos dois sentidos. O `>= 0` no Where é
    // guarda no BANCO: descurtir a mais não casa nenhuma linha em vez de deixar o contador negativo.
    public Task AjustarCurtidasAsync(Guid id, int delta, CancellationToken ct = default) =>
        db.Posts.Where(p => p.Id == id && p.QtdCurtidas + delta >= 0)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.QtdCurtidas, p => p.QtdCurtidas + delta), ct);

    // StartsWith vira `LIKE 'prefixo%'` no Postgres — pega "meditar" e "meditar-2" de uma vez.
    public async Task<IReadOnlyList<string>> ListarSlugsComPrefixoAsync(string prefixo, CancellationToken ct = default) =>
        await db.Posts.AsNoTracking()
                      .Where(p => p.Slug.StartsWith(prefixo))
                      .Select(p => p.Slug)
                      .ToListAsync(ct);

    // Include(Tags) sem AsNoTracking: o handler de edição precisa da coleção original
    // carregada, senão o EF não sabe quais linhas de post_tags remover.
    public Task<Post?> ObterParaEdicaoAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

    // Só rastreia. O SaveChanges fica pro UnitOfWorkBehavior (Fatia 5).
    public async Task AdicionarAsync(Post post, CancellationToken ct = default) =>
        await db.Posts.AddAsync(post, ct);

    public void Remover(Post post) => db.Posts.Remove(post);
}
