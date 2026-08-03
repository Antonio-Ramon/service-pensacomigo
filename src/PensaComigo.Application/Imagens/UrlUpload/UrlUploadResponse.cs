namespace PensaComigo.Application.Imagens.UrlUpload;

/// <summary>
/// <paramref name="Path"/> é o que o autor guarda depois no bloco de imagem / capa do post;
/// <paramref name="UrlAssinada"/> é descartável — serve pro PUT do upload e expira.
/// </summary>
public record UrlUploadResponse(string Path, string UrlAssinada);
