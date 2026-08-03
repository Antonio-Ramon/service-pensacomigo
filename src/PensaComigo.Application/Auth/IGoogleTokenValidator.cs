namespace PensaComigo.Application.Auth;

/// <summary>Dados que extraímos de um token Google já validado.</summary>
public record GoogleUserInfo(string Email, string GoogleId, string? FotoUrl);

/// <summary>
/// Seam: valida a assinatura do token do Google e devolve o usuário.
/// Impl real (Google.Apis.Auth) chega na Fatia 10; nos testes de integração
/// esta interface é trocada por um fake — não dá pra bater no Google de verdade.
/// </summary>
public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidarAsync(string idToken, CancellationToken ct = default);
}
