using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class TagRepository(PensaComigoDbContext db) : ITagRepository
{
    public Task<Tag?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);

    /// <summary>Whitelist do que o cliente pode filtrar/ordenar via querystring. Nada fora daqui.</summary>
    private static readonly IGridifyMapper<Tag> Mapper = new GridifyMapper<Tag>()
        .AddMap("nome", t => t.Nome)
        .AddMap("slug", t => t.Slug);

    public async Task<Pagina<Tag>> ListarAsync(IGridifyQuery consulta, CancellationToken ct = default)
    {
        // Sem ordem explícita a paginação fica instável (o banco não garante a mesma página duas vezes).
        if (string.IsNullOrWhiteSpace(consulta.OrderBy)) consulta.OrderBy = "nome";

        var (total, query) = await db.Tags.AsNoTracking().GridifyQueryableAsync(consulta, Mapper, ct);
        return new Pagina<Tag>(await query.ToListAsync(ct), total);
    }

    public Task<bool> ExistePorSlugAsync(string slug, CancellationToken ct = default) =>
        db.Tags.AnyAsync(t => t.Slug == slug, ct);

    public async Task AdicionarAsync(Tag tag, CancellationToken ct = default) =>
        await db.Tags.AddAsync(tag, ct);
}
