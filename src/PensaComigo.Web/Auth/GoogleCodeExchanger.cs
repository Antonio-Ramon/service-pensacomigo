using System.Text.Json;
using PensaComigo.Application.Auth;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Auth;

/// <summary>Impl real do seam: POST no endpoint de token do Google com o client_secret.
/// O id_token que volta ainda passa pelo <see cref="GoogleTokenValidator"/> no handler de login.</summary>
public class GoogleCodeExchanger(HttpClient http, IConfiguration config) : IGoogleCodeExchanger
{
    public async Task<string> TrocarCodePorIdTokenAsync(string code, string redirectUri, CancellationToken ct = default)
    {
        var resp = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = config["Google:ClientId"]!,
                ["client_secret"] = config["Google:ClientSecret"]!,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code",
            }), ct);

        if (!resp.IsSuccessStatusCode)
            throw new NaoAutorizadoException("Não foi possível concluir o login com o Google.");

        using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        return json.RootElement.GetProperty("id_token").GetString()!;
    }
}
