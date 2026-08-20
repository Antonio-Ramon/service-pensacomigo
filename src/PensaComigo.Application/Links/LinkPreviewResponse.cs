namespace PensaComigo.Application.Links;

/// <summary>Preview Open Graph pro bloco de link do editor. Nada disso é persistido aqui:
/// o front copia pro bloco e quem grava é o autor ao salvar o post.</summary>
public record LinkPreviewResponse(
    string Url,
    string? Titulo,
    string? Descricao,
    string? Thumbnail,
    string? SiteName);
