namespace PensaComigo.Application.Links;

/// <summary>Seam do fetch externo do preview (issue #21). A impl real (Web) faz o HTTP com a
/// guarda de SSRF por salto; os testes trocam por fake.</summary>
public interface IBuscadorPaginaExterna
{
    Task<string> BaixarHtmlAsync(string url, CancellationToken ct = default);
}
