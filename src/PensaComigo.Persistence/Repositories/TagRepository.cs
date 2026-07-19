using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class TagRepository(PensaComigoDbContext db) : ITagRepository
{
    public Task<Tag?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AdicionarAsync(Tag tag, CancellationToken ct = default) =>
        await db.Tags.AddAsync(tag, ct);
}
