using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace PensaComigo.Web.Visitantes;

/// <summary>
/// Identidade do leitor anônimo: IP + User-Agent, hasheados. Calculado NO SERVIDOR,
/// pelo mesmo motivo do path da imagem na Fatia 14 — se o cliente mandasse o hash,
/// bastaria trocar de valor para zerar o rate limit (e, nos likes, curtir de novo).
/// <para>
/// O hash evita guardar IP cru, que é dado pessoal — mas só com o segredo: SHA-256 puro sobre
/// IP+User-Agent é reversível por força bruta (~2³² IPs vezes uma lista curta de UAs), e o
/// resultado fica PERSISTIDO em <c>likes.viewer_hash</c>. Por isso HMAC com pepper.
/// </para>
/// </summary>
public sealed class HashVisitante(IOptions<VisitantesOptions> opcoes)
{
    private readonly byte[] _pepper = Encoding.UTF8.GetBytes(opcoes.Value.Pepper);

    public string De(HttpContext ctx)
    {
        var bruto = $"{ctx.Connection.RemoteIpAddress}|{ctx.Request.Headers.UserAgent}";

        return Convert.ToHexStringLower(HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(bruto)));
    }
}
