using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence;

public class PensaComigoDbContext : DbContext
{
    public PensaComigoDbContext(DbContextOptions<PensaComigoDbContext> options)
        : base(options)
    {
    }

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
