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
        builder.Property(u => u.IsAdmin).HasColumnName("is_admin").HasDefaultValue(false);
        builder.Property(u => u.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.GoogleId).IsUnique();

        // 1:N — um Usuario escreve muitos Posts (lado inverso configurado aqui)
        builder.HasMany(u => u.Posts)
               .WithOne(p => p.Autor)
               .HasForeignKey(p => p.AutorId);
    }
}
