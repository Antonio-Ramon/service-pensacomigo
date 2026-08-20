using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Nome).HasColumnName("nome").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").IsRequired();
        builder.Property(u => u.GoogleId).HasColumnName("google_id");
        builder.Property(u => u.ImagemUrl).HasColumnName("imagem_url").IsRequired();
        builder.Property(u => u.Bio).HasColumnName("bio");
        builder.Property(u => u.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
        builder.Property(u => u.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.GoogleId).IsUnique();

        // 1:N — um Usuario escreve muitos Posts (lado inverso configurado aqui)
        builder.HasMany(u => u.Posts)
               .WithOne(p => p.Autor)
               .HasForeignKey(p => p.AutorId);

        // Seed dos autores. Guid e DataCriacao FIXOS: HasData exige PK constante e
        // valor explícito para colunas obrigatórias (o now() default não vale aqui).
        builder.HasData(
            new Usuario
            {
                Id = new Guid("a1000000-0000-0000-0000-000000000001"),
                Nome = "Antonio Ramon",
                Email = "ar7339347@gmail.com",
                ImagemUrl = "",
                Bio = "Escreve em Pensa Comigo sobre fé que se pensa — meditações que se aproximam de pregações escritas.",
                IsAdmin = true,
                DataCriacao = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            },
            new Usuario
            {
                Id = new Guid("a1000000-0000-0000-0000-000000000002"),
                Nome = "Jessica Rose",
                Email = "jessicarosesc@gmail.com",
                ImagemUrl = "",
                Bio = "Escreve em Pensa Comigo meditações reflexivas — a fé que te obriga a pensar.",
                IsAdmin = true,
                DataCriacao = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
    }
}
