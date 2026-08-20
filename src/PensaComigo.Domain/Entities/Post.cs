using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string Slug { get; set; } = null!;

    /// <summary>Subtítulo curto (dek): frase abaixo do título, sem entregar a conclusão.</summary>
    public string? Dek { get; set; }

    /// <summary>Estados de chegada que a meditação atende — persistido como integer[].</summary>
    public List<Mood> Moods { get; set; } = [];

    // Etapa da trilha de leitura (opcional)
    public Guid? EtapaId { get; set; }
    public Etapa? Etapa { get; set; }

    // Array de blocos, persistido como jsonb (complex type do EF 10)
    public List<Bloco> Conteudo { get; set; } = [];

    public string ImagemCapa { get; set; } = null!;
    public int TempoLeitura { get; set; }         // minutos, calculado no Handler
    public int QtdCurtidas { get; set; }          // contador desnormalizado
    public int QtdVisualizacoes { get; set; }     // contador desnormalizado

    public Guid AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;

    public StatusPost Status { get; set; }

    // Congela na PRIMEIRA publicação (republicar não muda) — é a data que o feed ordena.
    public DateTime? DataPublicacao { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }

    // Navegações
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<Comentario> Comentarios { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
}