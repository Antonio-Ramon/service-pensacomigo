using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class EtapaRepository(PensaComigoDbContext db) : IEtapaRepository
{
    public async Task<IReadOnlyList<Etapa>> ListarAsync(CancellationToken ct = default) =>
        await db.Etapas.AsNoTracking().OrderBy(e => e.Numero).ToListAsync(ct);

    public Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Etapas.AnyAsync(e => e.Id == id, ct);
}
