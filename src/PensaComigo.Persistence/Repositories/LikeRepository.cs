using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class LikeRepository(PensaComigoDbContext db) : ILikeRepository
{
    public Task<bool> ExisteAsync(Guid postId, string viewerHash, CancellationToken ct = default) =>
        db.Likes.AnyAsync(l => l.PostId == postId && l.ViewerHash == viewerHash, ct);

    // Sem AsNoTracking: o handler de descurtir vai chamar Remover nesta instância.
    public Task<Like?> ObterAsync(Guid postId, string viewerHash, CancellationToken ct = default) =>
        db.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.ViewerHash == viewerHash, ct);

    // Só rastreia. O SaveChanges fica pro UnitOfWorkBehavior (Fatia 5).
    public async Task AdicionarAsync(Like like, CancellationToken ct = default) =>
        await db.Likes.AddAsync(like, ct);

    public void Remover(Like like) => db.Likes.Remove(like);
}
