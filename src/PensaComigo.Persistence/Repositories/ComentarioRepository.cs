using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class ComentarioRepository(PensaComigoDbContext db) : IComentarioRepository
{
    public Task<Comentario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Comentarios.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <summary>Whitelist da listagem. `aprovado`, `postId` e `parentId` NÃO estão aqui
    /// de propósito: são o recorte fixo do endpoint, não filtro do cliente — se fossem
    /// mapeados, `?filter=aprovado=false` viraria um painel de moderação anônimo.</summary>
    private static readonly IGridifyMapper<Comentario> Mapper = new GridifyMapper<Comentario>()
        .AddMap("autor", c => c.Autor)
        .AddMap("dataCriacao", c => c.DataCriacao);

    public async Task<Pagina<Comentario>> ListarAprovadosAsync(
        Guid postId, IGridifyQuery consulta, CancellationToken ct = default)
    {
        // Conversa se lê do começo pro fim (ao contrário do feed, que é do mais novo).
        if (string.IsNullOrWhiteSpace(consulta.OrderBy)) consulta.OrderBy = "dataCriacao";

        // Pagina só as RAÍZES: 20 comentários, não 20 linhas soltas com respostas cortadas
        // no meio. As respostas vêm por filtered include (`Where` dentro do Include) —
        // vira LEFT JOIN com a condição, não N+1 nem filtro em memória.
        var raizes = db.Comentarios.AsNoTracking()
            .Where(c => c.PostId == postId && c.Aprovado && c.ParentId == null)
            .Include(c => c.Respostas.Where(r => r.Aprovado).OrderBy(r => r.DataCriacao));

        var (total, query) = await raizes.GridifyQueryableAsync(consulta, Mapper, ct);

        return new Pagina<Comentario>(await query.ToListAsync(ct), total);
    }

    public async Task AdicionarAsync(Comentario comentario, CancellationToken ct = default) =>
        await db.Comentarios.AddAsync(comentario, ct);

    public void Remover(Comentario comentario) => db.Comentarios.Remove(comentario);
}
