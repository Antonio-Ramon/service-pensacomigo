namespace PensaComigo.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public DateTime DataCriacao { get; set; }

    // N:N com Post (a tabela de junção post_tags é criada pela configuration)
    public ICollection<Post> Posts { get; set; } = [];
}