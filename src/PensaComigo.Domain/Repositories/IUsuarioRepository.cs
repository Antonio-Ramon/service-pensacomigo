using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
}
