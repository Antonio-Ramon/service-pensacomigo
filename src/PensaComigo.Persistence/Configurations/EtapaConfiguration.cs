using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Persistence.Configurations;

public class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> builder)
    {
        builder.ToTable("etapas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Numero).HasColumnName("numero");
        builder.Property(e => e.Titulo).HasColumnName("titulo").IsRequired();
        builder.Property(e => e.Descricao).HasColumnName("descricao").IsRequired();
        builder.Property(e => e.Refs).HasColumnName("refs");

        builder.HasIndex(e => e.Numero).IsUnique();

        // Trilha "da pergunta ao descanso" — seed do catálogo (ids fixos p/ migration determinística).
        builder.HasData(
            new Etapa { Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"), Numero = 1, Titulo = "A Pergunta", Descricao = "Nomear o que aperta: a dúvida, a dor, o que não cala." },
            new Etapa { Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"), Numero = 2, Titulo = "A Busca", Descricao = "Levar a pergunta ao texto: ler devagar, sem atalho." },
            new Etapa { Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"), Numero = 3, Titulo = "O Encontro", Descricao = "Deixar o texto responder — e mudar a pergunta." },
            new Etapa { Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"), Numero = 4, Titulo = "O Descanso", Descricao = "Guardar o que foi dado e descansar nele." });
    }
}
