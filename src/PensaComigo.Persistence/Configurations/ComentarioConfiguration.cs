using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence.Configurations;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("comentarios");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.PostId).HasColumnName("post_id");
        builder.Property(c => c.ParentId).HasColumnName("parent_id");
        builder.Property(c => c.Autor).HasColumnName("autor").IsRequired();
        builder.Property(c => c.Conteudo).HasColumnName("conteudo").IsRequired();
        builder.Property(c => c.Aprovado).HasColumnName("aprovado").HasDefaultValue(false);
        builder.Property(c => c.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");

        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.ParentId);

        // 1:N — comentário pertence a um Post (cascade delete)
        builder.HasOne(c => c.Post)
               .WithMany(p => p.Comentarios)
               .HasForeignKey(c => c.PostId)
               .OnDelete(DeleteBehavior.Cascade);

        // Auto-referência — resposta aponta para o comentário pai
        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Respostas)
               .HasForeignKey(c => c.ParentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
