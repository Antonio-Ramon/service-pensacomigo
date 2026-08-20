using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Auth.Login;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Controllers;

/// <summary>
/// O backend conduz o OAuth inteiro (issue #17): o front só chama <c>google/iniciar</c> e
/// recebe a sessão como cookie httpOnly — nunca vê id_token nem JWT.
/// O <c>POST login</c> antigo (id_token no corpo) fica por compatibilidade.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(ISender mediator, IConfiguration config, IWebHostEnvironment env) : ControllerBase
{
    public const string CookieSessao = "pc_sessao";
    private const string CookieOAuth = "pc_oauth";

    /// <summary>Controller magro: recebe, delega ao MediatR, devolve. Zero regra aqui.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginGoogleCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    /// <summary>Início do fluxo: redireciona pro consent do Google. O state (anti-CSRF) e o
    /// destino final ficam num cookie curto que o callback confere.</summary>
    [HttpGet("google/iniciar")]
    [AllowAnonymous]
    public IActionResult IniciarGoogle([FromQuery] string? returnUrl)
    {
        var destino = ResolverDestino(returnUrl);
        var state = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(CookieOAuth, $"{state}|{destino}", new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,   // o retorno do Google é navegação top-level GET: Lax passa
            MaxAge = TimeSpan.FromMinutes(10),
            Path = "/api/v1/auth",
        });

        var query = new QueryString()
            .Add("client_id", config["Google:ClientId"]!)
            .Add("redirect_uri", UrlCallback())
            .Add("response_type", "code")
            .Add("scope", "openid email profile")
            .Add("state", state);

        return Redirect("https://accounts.google.com/o/oauth2/v2/auth" + query);
    }

    /// <summary>Volta do Google: confere o state, troca o code pelo id_token, valida e
    /// resolve o usuário (mesmo pipeline do login antigo) e emite a sessão como cookie.</summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> CallbackGoogle(
        [FromQuery] string code, [FromQuery] string state,
        [FromServices] IGoogleCodeExchanger exchanger, CancellationToken ct)
    {
        var cookie = Request.Cookies[CookieOAuth];
        Response.Cookies.Delete(CookieOAuth, new CookieOptions { Path = "/api/v1/auth" });

        if (string.IsNullOrEmpty(state) || cookie is null || !cookie.StartsWith(state + "|"))
            throw new NaoAutorizadoException("Fluxo de login inválido ou expirado. Comece de novo.");

        var destino = cookie[(state.Length + 1)..];

        var idToken = await exchanger.TrocarCodePorIdTokenAsync(code, UrlCallback(), ct);
        var login = await mediator.Send(new LoginGoogleCommand(idToken), ct);

        Response.Cookies.Append(CookieSessao, login.Token, OpcoesCookieSessao(
            // MaxAge acompanha a validade do JWT dentro do cookie (8h, sem refresh — como antes).
            maxAge: TimeSpan.FromHours(8)));

        return Redirect(destino);
    }

    /// <summary>Logout: expira o cookie de sessão. O JWT em si só morre no vencimento (8h).</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieSessao, OpcoesCookieSessao(maxAge: null));
        return NoContent();
    }

    private CookieOptions OpcoesCookieSessao(TimeSpan? maxAge)
    {
        // SameSite=None (front em domínio diferente) exige Secure — o browser descarta sem HTTPS.
        var crossSite = config.GetValue("Auth:CookieCrossSite", false);
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = crossSite || !env.IsDevelopment(),
            SameSite = crossSite ? SameSiteMode.None : SameSiteMode.Lax,
            MaxAge = maxAge,
            Path = "/",
        };
    }

    // Guarda contra open redirect: só devolve o leitor para uma origem do front conhecida.
    private string ResolverDestino(string? returnUrl)
    {
        var origens = config.GetSection("OrigensFront").Get<string[]>() ?? [];
        if (returnUrl is not null &&
            origens.Any(o => returnUrl == o || returnUrl.StartsWith(o + "/", StringComparison.Ordinal)))
            return returnUrl;

        return origens.FirstOrDefault() ?? "/";
    }

    // Atrás de proxy o UseForwardedHeaders já reescreveu scheme/host.
    private string UrlCallback() => $"{Request.Scheme}://{Request.Host}/api/v1/auth/google/callback";
}
