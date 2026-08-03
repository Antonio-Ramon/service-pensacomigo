using MediatR;
using PensaComigo.Application.Storage;

namespace PensaComigo.Application.Imagens.UrlUpload;

/// <summary>
/// Monta o path NO SERVIDOR e manda o storage assiná-lo. URL assinada é permissão de escrita:
/// se o cliente escolhesse o path, pediria uma na pasta de outro autor. Só a extensão vem de fora
/// (já filtrada pelo validator); o dono vem da claim e o nome é um Guid novo.
/// </summary>
public class GerarUrlUploadQueryHandler(IStorage storage)
    : IRequestHandler<GerarUrlUploadQuery, UrlUploadResponse>
{
    public async Task<UrlUploadResponse> Handle(GerarUrlUploadQuery q, CancellationToken ct)
    {
        var extensao = Path.GetExtension(q.NomeArquivo).ToLowerInvariant();
        var path = $"posts/{q.UsuarioId}/{Guid.NewGuid()}{extensao}";

        return new UrlUploadResponse(path, await storage.GerarUrlUploadAssinadaAsync(path, ct));
    }
}
