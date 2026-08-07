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
        .AddMap("titulo", p => p.Titulo)
        .AddMap("slug", p => p.Slug)
        .AddMap("autor", p => p.Autor.Nome)
        .AddMap("tag", p => p.Tags.Select(t => t.Slug))
        .AddMap("dataCriacao", p => p.DataCriacao);

    public async Task<Pagina<Post>> ListarAsync(IGridifyQuery consulta, CancellationToken ct = default)
    {
        // Feed é cronológico: mais novo primeiro. Sem ordem explícita a paginação fica instável.
        if (string.IsNullOrWhiteSpace(consulta.OrderBy)) consulta.OrderBy = "dataCriacao desc";

        var (total, query) = await db.Posts.AsNoTracking().GridifyQueryableAsync(consulta, Mapper, ct);
        return new Pagina<Post>(await query.ToListAsync(ct), total);
    }

    public Task<Post?> ObterPorSlugAsync(string slug, CancellationToken ct = default) =>
        db.Posts.AsNoTracking()
                .Include(p => p.Autor)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    // UPDATE ... SET qtd_visualizacoes = qtd_visualizacoes + 1 direto no banco.
    // Ler-somar-gravar pelo change tracker perderia contagem com dois leitores simultâneos.
    public Task IncrementarVisualizacoesAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.Where(p => p.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.QtdVisualizacoes, p => p.QtdVisualizacoes + 1), ct);

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
