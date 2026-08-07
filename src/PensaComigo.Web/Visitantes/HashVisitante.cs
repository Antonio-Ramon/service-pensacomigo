using System.Security.Cryptography;
using System.Text;

namespace PensaComigo.Web.Visitantes;

/// <summary>
/// Identidade do leitor anônimo: IP + User-Agent, hasheados. Calculado NO SERVIDOR,
/// pelo mesmo motivo do path da imagem na Fatia 14 — se o cliente mandasse o hash,
/// bastaria trocar de valor para zerar o rate limit (e, nos likes, curtir de novo).
/// O hash evita guardar IP cru, que é dado pessoal.
/// </summary>
public static class HashVisitante
{
    public static string De(HttpContext ctx)
    {
        var bruto = $"{ctx.Connection.RemoteIpAddress}|{ctx.Request.Headers.UserAgent}";

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(bruto)));
    }
}
