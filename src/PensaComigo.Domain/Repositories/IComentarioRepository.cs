using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IComentarioRepository
{
    Task<Comentario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Comentario comentario, CancellationToken ct = default);
}
