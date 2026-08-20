using AngleSharp.Html.Parser;
using PensaComigo.Application.Links;

namespace PensaComigo.Application.Common;

/// <summary>Função pura: HTML → metadados Open Graph, com fallback pra &lt;title&gt; e
/// meta description. AngleSharp já vem de carona com o HtmlSanitizer.</summary>
public static class ExtratorOpenGraph
{
    public static LinkPreviewResponse Extrair(string url, string html)
    {
        var doc = new HtmlParser().ParseDocument(html);

        string? Og(string prop) =>
            doc.QuerySelector($"meta[property='og:{prop}']")?.GetAttribute("content");

        var titulo = Og("title") ?? doc.QuerySelector("title")?.TextContent;
        var descricao = Og("description")
            ?? doc.QuerySelector("meta[name='description']")?.GetAttribute("content");

        return new LinkPreviewResponse(
            url,
            Limpar(titulo),
            Limpar(descricao),
            Limpar(Og("image")),
            Limpar(Og("site_name")));
    }

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
