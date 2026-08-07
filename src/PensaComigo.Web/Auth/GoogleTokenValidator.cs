using Google.Apis.Auth;
using PensaComigo.Application.Auth;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Auth;

/// <summary>
/// Impl real do seam da Fatia 9. A lib do Google confere a assinatura do idToken
/// contra as chaves públicas do Google e valida que o <c>aud</c> é o nosso ClientId.
/// A Application nunca vê esta lib — é por isso que o seam existe.
/// </summary>
public class GoogleTokenValidator(IConfiguration config) : IGoogleTokenValidator
{
    public async Task<GoogleUserInfo> ValidarAsync(string idToken, CancellationToken ct = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [config["Google:ClientId"]!],
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            throw new NaoAutorizadoException("Token do Google inválido ou expirado.");
        }

        return new GoogleUserInfo(payload.Email, payload.Subject, payload.Picture);
    }
}
