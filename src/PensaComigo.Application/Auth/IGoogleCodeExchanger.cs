namespace PensaComigo.Application.Auth;

/// <summary>
/// Seam do passo servidor-a-servidor do OAuth (issue #17): troca o <c>code</c> do callback
/// pelo <c>id_token</c> no endpoint de token do Google. A impl real (HttpClient) vive na Web;
/// os testes de integração trocam por fake.
/// </summary>
public interface IGoogleCodeExchanger
{
    Task<string> TrocarCodePorIdTokenAsync(string code, string redirectUri, CancellationToken ct = default);
}
