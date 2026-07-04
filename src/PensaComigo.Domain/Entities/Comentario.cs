namespace PensaComigo.Domain.Entities;

public class Comentario
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    // null = comentário raiz. Preenchido = resposta (limitado a 1 nível
    // pela regra de negócio no Handler/Validator, não pelo schema).
    public Guid? ParentId { get; set; }
    public Comentario? Parent { get; set; }
    public ICollection<Comentario> Respostas { get; set; } = [];

    public string Autor { get; set; } = null!;
    public string Conteudo { get; set; } = null!;
    public bool Aprovado { get; set; }
    public DateTime DataCriacao { get; set; }
}