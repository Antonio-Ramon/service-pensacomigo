namespace PensaComigo.Domain.Entities;

public class Like
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    // Hash do visitante — impede curtida duplicada (constraint única na configuration)
    public string ViewerHash { get; set; } = null!;

    public DateTime DataCriacao { get; set; }
}