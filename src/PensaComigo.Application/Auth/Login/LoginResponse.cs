namespace PensaComigo.Application.Auth.Login;

/// <summary>O que o login devolve pro frontend: o JWT próprio + o perfil.</summary>
public record LoginResponse(string Token, string Nome, string? ImagemUrl);
