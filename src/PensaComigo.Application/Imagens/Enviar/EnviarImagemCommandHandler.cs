using MediatR;
using PensaComigo.Application.Storage;

namespace PensaComigo.Application.Imagens.Enviar;

/// <summary>
/// Monta o path NO SERVIDOR e manda os bytes pro storage. O dono da pasta vem da claim e o
/// nome é um Guid novo: nome de arquivo do cliente é entrada hostil (path traversal, colisão,
/// unicode esquisito) e aqui só a extensão dele sobrevive.
/// </summary>
public class EnviarImagemCommandHandler(IStorage storage)
    : IRequestHandler<EnviarImagemCommand, ImagemResponse>
{
    public async Task<ImagemResponse> Handle(EnviarImagemCommand cmd, CancellationToken ct)
    {
        var extensao = Path.GetExtension(cmd.NomeArquivo).ToLowerInvariant();
        var path = $"posts/{cmd.UsuarioId}/{Guid.NewGuid()}{extensao}";

        var url = await storage.EnviarAsync(path, cmd.Conteudo, ImagensPermitidas.Tipos[extensao], ct);

        return new ImagemResponse(path, url);
    }
}
