using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Titulo).HasColumnName("titulo").IsRequired();
        builder.Property(p => p.Dek).HasColumnName("dek").HasMaxLength(200);
        // Npgsql mapeia List<enum> para integer[] nativo — filtrável com = ANY(moods).
        builder.Property(p => p.Moods).HasColumnName("moods");
        builder.Property(p => p.EtapaId).HasColumnName("etapa_id");
        builder.Property(p => p.Slug).HasColumnName("slug").IsRequired();
        builder.Property(p => p.ImagemCapa).HasColumnName("imagem_capa").IsRequired();
        builder.Property(p => p.TempoLeitura).HasColumnName("tempo_leitura").HasDefaultValue(0);
        builder.Property(p => p.QtdCurtidas).HasColumnName("qtd_curtidas").HasDefaultValue(0);
        builder.Property(p => p.QtdVisualizacoes).HasColumnName("qtd_visualizacoes").HasDefaultValue(0);
        builder.Property(p => p.AutorId).HasColumnName("autor_id");
        builder.Property(p => p.Status).HasColumnName("status").HasDefaultValue(Domain.Enums.StatusPost.Rascunho);
        builder.Property(p => p.DataPublicacao).HasColumnName("data_publicacao");
        builder.Property(p => p.DataCriacao).HasColumnName("data_criacao").HasDefaultValueSql("now()");
        builder.Property(p => p.DataAtualizacao).HasColumnName("data_atualizacao").HasDefaultValueSql("now()");

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.DataCriacao);
        builder.HasIndex(p => p.DataPublicacao);   // ordem padrão do feed

        // Fatia 2 aprofunda: List<Bloco> serializado como coluna jsonb (blob).
        // ValueComparer garante que o EF detecte mutação dentro da lista.
        var opts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var comparer = new ValueComparer<List<Bloco>>(
            (a, b) => JsonSerializer.Serialize(a, opts) == JsonSerializer.Serialize(b, opts),
            v => JsonSerializer.Serialize(v, opts).GetHashCode(),
            v => JsonSerializer.Deserialize<List<Bloco>>(JsonSerializer.Serialize(v, opts), opts)!);

        builder.Property(p => p.Conteudo)
               .HasColumnName("conteudo")
               .HasColumnType("jsonb")
               .HasConversion(
                   v => JsonSerializer.Serialize(v, opts),
                   v => JsonSerializer.Deserialize<List<Bloco>>(v, opts) ?? new List<Bloco>())
               .Metadata.SetValueComparer(comparer);

        // Etapa é catálogo: apagar etapa em uso é bloqueado pelo banco (Restrict).
        builder.HasOne(p => p.Etapa)
               .WithMany()
               .HasForeignKey(p => p.EtapaId)
               .OnDelete(DeleteBehavior.Restrict);

        // N:N com Tag → tabela de junção post_tags
        builder.HasMany(p => p.Tags)
               .WithMany(t => t.Posts)
               .UsingEntity(
                   "post_tags",
                   l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("tag_id").OnDelete(DeleteBehavior.Cascade),
                   r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("post_id").OnDelete(DeleteBehavior.Cascade));
    }
}
