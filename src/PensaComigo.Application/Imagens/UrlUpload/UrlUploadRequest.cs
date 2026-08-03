namespace PensaComigo.Application.Imagens.UrlUpload;

/// <summary>
/// Corpo do endpoint. Só o nome do arquivo — o autor vem da claim `sub`, não do corpo
/// (senão um autor pediria URL de escrita na pasta de outro).
/// </summary>
public record UrlUploadRequest(string NomeArquivo);
