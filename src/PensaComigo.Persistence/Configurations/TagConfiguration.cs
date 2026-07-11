using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Nome).HasColumnName("nome").IsRequired();
        builder.Property(t => t.Slug).HasColumnName("slug").IsRequired();
        builder.Property(t => t.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");

        builder.HasIndex(t => t.Nome).IsUnique();
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
