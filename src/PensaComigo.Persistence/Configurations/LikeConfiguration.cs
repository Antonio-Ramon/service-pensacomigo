using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence.Configurations;

public class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.ToTable("likes");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.PostId).HasColumnName("post_id");
        builder.Property(l => l.ViewerHash).HasColumnName("viewer_hash").IsRequired();
        builder.Property(l => l.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");

        // Impede curtida duplicada do mesmo visitante no mesmo post
        builder.HasIndex(l => new { l.PostId, l.ViewerHash }).IsUnique();

        builder.HasOne(l => l.Post)
               .WithMany(p => p.Likes)
               .HasForeignKey(l => l.PostId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
