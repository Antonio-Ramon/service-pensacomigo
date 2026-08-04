using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using PensaComigo.Application.Storage;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Storage;

/// <summary>
/// Impl do seam no host. O <c>HttpClient</c> chega pronto (BaseAddress + Bearer) porque
/// foi registrado como typed client no <c>Program.cs</c> — não se dá <c>new HttpClient()</c> aqui.
/// </summary>
public class SupabaseStorage(HttpClient http, IOptions<SupabaseOptions> options) : IStorage
{
    private readonly SupabaseOptions _cfg = options.Value;

    public async Task<string> EnviarAsync(string path, Stream conteudo, string contentType, CancellationToken ct)
    {
        // StreamContent repassa os bytes conforme chegam: o arquivo não vira byte[] na memória.
        using var corpo = new StreamContent(conteudo);
        corpo.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var resposta = await http.PostAsync($"object/{_cfg.Bucket}/{path}", corpo, ct);

        if (!resposta.IsSuccessStatusCode)
            throw new RegraDeNegocioException("Não foi possível enviar a imagem.");

        // Bucket público: leitura não precisa de assinatura, é URL estável derivada do path.
        return new Uri(http.BaseAddress!, $"object/public/{_cfg.Bucket}/{path}").ToString();
    }
}
