using System.Net.Http.Json;
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

    public async Task<string> GerarUrlUploadAssinadaAsync(string path, CancellationToken ct)
    {
        // Corpo vazio mesmo: quem identifica o arquivo é a rota (igual ao SDK storage-js).
        var resposta = await http.PostAsJsonAsync(
            $"object/upload/sign/{_cfg.Bucket}/{path}", new { }, ct);

        if (!resposta.IsSuccessStatusCode)
            throw new RegraDeNegocioException("Não foi possível gerar a URL de upload da imagem.");

        var corpo = await resposta.Content.ReadFromJsonAsync<UrlAssinada>(ct);

        // O Supabase devolve a url RELATIVA a /storage/v1; o front precisa da absoluta.
        return new Uri(http.BaseAddress!, corpo!.Url.TrimStart('/')).ToString();
    }

    private sealed record UrlAssinada(string Url);
}
