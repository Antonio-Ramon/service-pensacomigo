using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Persistence;

public class PensaComigoDbContext : DbContext, IUnitOfWork
{
    public PensaComigoDbContext(DbContextOptions<PensaComigoDbContext> options)
        : base(options)
    {
    }

    // IUnitOfWork: o commit atômico que o UnitOfWorkBehavior chama ao fim de um Command.
    public Task<int> CommitAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica toda IEntityTypeConfiguration da pasta Configurations automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PensaComigoDbContext).Assembly);
    }
}
