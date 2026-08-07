using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Moderar;

public class DeletarComentarioCommandHandler(IComentarioRepository comentarios)
    : IRequestHandler<DeletarComentarioCommand, Unit>
{
    public async Task<Unit> Handle(DeletarComentarioCommand cmd, CancellationToken ct)
    {
        var comentario = await comentarios.ObterPorIdAsync(cmd.Id, ct)
                         ?? throw new NaoEncontradoException("Comentário", cmd.Id);

        comentarios.Remover(comentario);

        return Unit.Value;
    }
}
