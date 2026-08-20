using MediatR;
using PensaComigo.Application.Common;

namespace PensaComigo.Application.Links.Preview;

/// <summary>Baixa a página (seam com guarda de SSRF) e extrai o Open Graph com função pura.</summary>
public class ObterPreviewLinkQueryHandler(IBuscadorPaginaExterna buscador)
    : IRequestHandler<ObterPreviewLinkQuery, LinkPreviewResponse>
{
    public async Task<LinkPreviewResponse> Handle(ObterPreviewLinkQuery q, CancellationToken ct)
    {
        var html = await buscador.BaixarHtmlAsync(q.Url, ct);
        return ExtratorOpenGraph.Extrair(q.Url, html);
    }
}
