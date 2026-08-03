using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Imagens.UrlUpload;

/// <summary>
/// Query, não Command: nada é gravado no nosso Postgres, então não há o que o
/// UnitOfWorkBehavior (Fatia 5) commitar. O <paramref name="UsuarioId"/> vem da claim `sub`.
/// </summary>
public record GerarUrlUploadQuery(Guid UsuarioId, string NomeArquivo) : IQuery<UrlUploadResponse>;
