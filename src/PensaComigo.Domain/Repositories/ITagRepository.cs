using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Tag tag, CancellationToken ct = default);
}
