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
        // unaccent nos dois lados: coluna no SQL, termo no C# (issue #30).
        .AddMap("nome", t => EF.Functions.Unaccent(t.Nome), v => Textos.RemoverAcentos(v))
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

    // SEM AsNoTracking de propósito: o handler pendura essas instâncias no Post e o
    // change tracker precisa saber que elas já existem (senão o EF tenta INSERT na tag).
    public Task<List<Tag>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        db.Tags.Where(t => ids.Contains(t.Id)).ToListAsync(ct);

    public async Task AdicionarAsync(Tag tag, CancellationToken ct = default) =>
        await db.Tags.AddAsync(tag, ct);

    // COUNT direto na junção: não carrega post nenhum pra memória.
    public Task<int> ContarPostsAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.CountAsync(p => p.Tags.Any(t => t.Id == id), ct);

    // Só rastreia a remoção. O SaveChanges fica pro UnitOfWorkBehavior.
    public void Remover(Tag tag) => db.Tags.Remove(tag);
}
