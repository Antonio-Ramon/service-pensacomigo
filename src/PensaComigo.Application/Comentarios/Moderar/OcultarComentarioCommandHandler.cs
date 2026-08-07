using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Moderar;

public class OcultarComentarioCommandHandler(IComentarioRepository comentarios)
    : IRequestHandler<OcultarComentarioCommand, Unit>
{
    public async Task<Unit> Handle(OcultarComentarioCommand cmd, CancellationToken ct)
    {
        // ObterPorIdAsync NÃO usa AsNoTracking: a entidade vem rastreada, então mudar
        // a propriedade já basta — o UPDATE sai no SaveChanges do UnitOfWorkBehavior.
        var comentario = await comentarios.ObterPorIdAsync(cmd.Id, ct)
                         ?? throw new NaoEncontradoException("Comentário", cmd.Id);

        comentario.Aprovado = false;

        // Respostas não precisam ser ocultadas uma a uma: a listagem só devolve
        // respostas DE raízes visíveis, então esconder o pai leva a conversa junto.
        return Unit.Value;
    }
}
