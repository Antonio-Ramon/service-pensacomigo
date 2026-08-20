using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IEtapaRepository
{
    /// <summary>Catálogo inteiro, ordenado por número — são 4 linhas, sem paginação.</summary>
    Task<IReadOnlyList<Etapa>> ListarAsync(CancellationToken ct = default);

    Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default);
}
