using System.Net;
using System.Text;
using PensaComigo.Application.Common;
using PensaComigo.Application.Links;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Links;

/// <summary>
/// Impl real do fetch do preview (issue #21). Redirects são seguidos NA MÃO (máx. 3) para
/// revalidar a guarda de SSRF a cada salto. Timeout de 5s e User-Agent próprio vêm do
/// HttpClient nomeado; nenhum header interno de auth é repassado.
/// </summary>
public class BuscadorPaginaExterna(IHttpClientFactory clientes) : IBuscadorPaginaExterna
{
    public const string ClienteHttp = "preview-links";
    private const int MaxRedirects = 3;
    private const int MaxBytes = 1024 * 1024;   // 1 MB: o OG mora no <head>, sobra de folga

    public async Task<string> BaixarHtmlAsync(string url, CancellationToken ct = default)
    {
        var http = clientes.CreateClient(ClienteHttp);
        var atual = url;

        for (var salto = 0; salto <= MaxRedirects; salto++)
        {
            var uri = await ValidarAsync(atual, ct);

            using var resp = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);

            if ((int)resp.StatusCode is >= 300 and < 400)
            {
                var destino = resp.Headers.Location
                    ?? throw new RegraDeNegocioException("O link redirecionou sem destino.");
                atual = destino.IsAbsoluteUri ? destino.ToString() : new Uri(uri, destino).ToString();
                continue;   // o próximo salto passa pela guarda de novo
            }

            if (!resp.IsSuccessStatusCode)
                throw new RegraDeNegocioException("A página do link não respondeu.");

            if (resp.Content.Headers.ContentType?.MediaType != "text/html")
                throw new RegraDeNegocioException("O link não aponta para uma página HTML.");

            return await LerAteOTetoAsync(resp, ct);
        }

        throw new RegraDeNegocioException("O link redireciona demais.");
    }

    // ponytail: valida DNS e depois deixa o HttpClient resolver de novo — janela teórica de
    // DNS rebinding entre os dois resolves. Fechar exigiria conectar no IP validado com
    // SocketsHttpHandler.ConnectCallback; fazer se o serviço um dia rodar perto de rede interna sensível.
    private static async Task<Uri> ValidarAsync(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new RegraDeNegocioException("A url do link precisa ser http ou https.");

        IPAddress[] ips;
        try
        {
            ips = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, ct);
        }
        catch (Exception)
        {
            throw new RegraDeNegocioException("Não foi possível resolver o endereço do link.");
        }

        if (ips.Length == 0 || ips.Any(ip => !GuardaSsrf.EnderecoPermitido(ip)))
            throw new RegraDeNegocioException("O link aponta para um endereço interno e foi bloqueado.");

        return uri;
    }

    private static async Task<string> LerAteOTetoAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var ms = new MemoryStream();

        var buffer = new byte[8192];
        int lidos;
        while ((lidos = await stream.ReadAsync(buffer, ct)) > 0)
        {
            // Corta no teto em vez de falhar: o que interessa (head/OG) já veio.
            if (ms.Length + lidos > MaxBytes) { ms.Write(buffer, 0, MaxBytes - (int)ms.Length); break; }
            ms.Write(buffer, 0, lidos);
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
