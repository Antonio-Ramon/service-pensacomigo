namespace PensaComigo.Domain.Repositories;

/// <summary>
/// Ponte entre a Application e o DbContext: o commit atômico de um caso de uso.
/// Implementado pelo PensaComigoDbContext; consumido pelo UnitOfWorkBehavior.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
