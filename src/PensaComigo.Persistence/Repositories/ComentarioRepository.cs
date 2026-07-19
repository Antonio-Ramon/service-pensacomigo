using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence.Repositories;

public class ComentarioRepository(PensaComigoDbContext db) : IComentarioRepository
{
    public Task<Comentario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        db.Comentarios.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AdicionarAsync(Comentario comentario, CancellationToken ct = default) =>
        await db.Comentarios.AddAsync(comentario, ct);
}
