using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Post post, CancellationToken ct = default);
}
