using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class PostRepository(PensaComigoDbContext db) : IPostRepository
{
    public Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);

    // Só rastreia. O SaveChanges fica pro UnitOfWorkBehavior (Fatia 5).
    public async Task AdicionarAsync(Post post, CancellationToken ct = default) =>
        await db.Posts.AddAsync(post, ct);
}
