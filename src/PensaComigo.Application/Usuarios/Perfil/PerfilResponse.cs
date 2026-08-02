namespace PensaComigo.Application.Usuarios.Perfil;

/// <summary>O perfil do usuário logado que o GET /me devolve.</summary>
public record PerfilResponse(Guid Id, string Nome, string Email, string ImagemUrl, bool IsAdmin);
