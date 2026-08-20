namespace PensaComigo.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Subject do token Google (preenchido no primeiro login). Nullable até lá.
    public string? GoogleId { get; set; }

    // Foto puxada da conta Google
    public string ImagemUrl { get; set; } = null!;

    // Bio exibida no "Quem escreve" da home e no rodapé do post (issue #22)
    public string? Bio { get; set; }

    // Expansível: novos autores além de Antonio e Jéssica
    public bool IsAdmin { get; set; } = true;

    public DateTime DataCriacao { get; set; }

    // Navegação: posts escritos por este usuário
    public ICollection<Post> Posts { get; set; } = [];
}