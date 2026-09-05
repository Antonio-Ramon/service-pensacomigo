using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Fluxo OAuth conduzido pelo backend (issue #17): iniciar → callback → sessão em cookie
/// httpOnly → rota autenticada via cookie → logout. Google é trocado por fakes nos dois
/// seams (code exchange e validação do id_token).
/// </summary>
public class AuthFluxoTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Iniciar_redireciona_pro_google_e_planta_cookie_de_state()
    {
        var client = ClienteSemRedirect();

        var resp = await client.GetAsync("/api/v1/auth/google/iniciar");

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth", resp.Headers.Location!.ToString());
        Assert.Contains("state=", resp.Headers.Location.ToString());
        Assert.Contains(resp.Headers.GetValues("Set-Cookie"), c => c.StartsWith("pc_oauth="));
    }

    [Fact]
    public async Task Callback_com_state_valido_emite_cookie_de_sessao_que_autentica()
    {
        FakeGoogle.Email = await EmailDoSeedAsync();
        var client = ClienteSemRedirect();

        // 1. iniciar: captura state e cookie
        var iniciar = await client.GetAsync("/api/v1/auth/google/iniciar");
        var (state, cookieOAuth) = ExtrairStateECookie(iniciar);

        // 2. callback: state confere → 302 pro destino + Set-Cookie pc_sessao
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/google/callback?code=fake-code&state={state}");
        req.Headers.Add("Cookie", cookieOAuth);
        var callback = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        var sessao = Assert.Single(callback.Headers.GetValues("Set-Cookie"), c => c.StartsWith("pc_sessao="));
        Assert.Contains("httponly", sessao, StringComparison.OrdinalIgnoreCase);

        // 3. o cookie (sem Authorization) autentica uma rota [Authorize]
        var valorSessao = sessao.Split(';')[0];
        var autenticada = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tags")
        {
            Content = JsonContent.Create(new { nome = $"Via cookie {Guid.NewGuid():N}" }),
        };
        autenticada.Headers.Add("Cookie", valorSessao);

        var respAutenticada = await client.SendAsync(autenticada);
        respAutenticada.EnsureSuccessStatusCode();
    }

    // O callback é navegação top-level: falha nunca pode virar JSON de erro na cara do leitor.
    [Theory]
    [InlineData("code=x&state=forjado", "expirado")]        // state não confere com o cookie
    [InlineData("error=access_denied", "cancelado")]        // usuário cancelou no consent
    public async Task Callback_que_falha_redireciona_pro_front_com_erro(string query, string motivo)
    {
        var client = ClienteSemRedirect();
        var iniciar = await client.GetAsync("/api/v1/auth/google/iniciar");
        var (_, cookieOAuth) = ExtrairStateECookie(iniciar);

        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/google/callback?{query}");
        req.Headers.Add("Cookie", cookieOAuth);
        var resp = await client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);
        Assert.Contains($"erro={motivo}", resp.Headers.Location!.ToString());
        Assert.DoesNotContain(resp.Headers.GetValues("Set-Cookie"), c => c.StartsWith("pc_sessao="));
    }

    [Fact]
    public async Task Logout_expira_o_cookie_de_sessao()
    {
        var resp = await ClienteSemRedirect().PostAsync("/api/v1/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        var cookie = Assert.Single(resp.Headers.GetValues("Set-Cookie"), c => c.StartsWith("pc_sessao="));
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);   // no passado = apagado
    }

    private HttpClient ClienteSemRedirect() => factory
        .WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.AddScoped<IGoogleCodeExchanger, FakeGoogle>();
            s.AddScoped<IGoogleTokenValidator, FakeGoogle>();
        }))
        .CreateClient(new() { AllowAutoRedirect = false });

    private static (string State, string CookieOAuth) ExtrairStateECookie(HttpResponseMessage iniciar)
    {
        var query = System.Web.HttpUtility.ParseQueryString(iniciar.Headers.Location!.Query);
        var setCookie = iniciar.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("pc_oauth="));
        return (query["state"]!, setCookie.Split(';')[0]);
    }

    private async Task<string> EmailDoSeedAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        return (await db.Usuarios.AsNoTracking().FirstAsync(u => u.Nome.StartsWith("Antonio"))).Email;
    }

    /// <summary>Fake dos dois seams: devolve um id_token de mentira e o "valida" pro email do seed.</summary>
    private sealed class FakeGoogle : IGoogleCodeExchanger, IGoogleTokenValidator
    {
        public static string Email = "";

        public Task<string> TrocarCodePorIdTokenAsync(string code, string redirectUri, CancellationToken ct = default) =>
            Task.FromResult("id-token-fake");

        public Task<GoogleUserInfo> ValidarAsync(string idToken, CancellationToken ct = default) =>
            Task.FromResult(new GoogleUserInfo(Email, "google-id-fake", null));
    }
}
