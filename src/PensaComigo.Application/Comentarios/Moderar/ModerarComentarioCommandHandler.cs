using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Moderar;

public class ModerarComentarioCommandHandler(IComentarioRepository comentarios)
    : IRequestHandler<ModerarComentarioCommand, Unit>
{
    public async Task<Unit> Handle(ModerarComentarioCommand cmd, CancellationToken ct)
    {
        // ObterPorIdAsync NÃO usa AsNoTracking: a entidade vem rastreada, então mudar
        // a propriedade já basta — o UPDATE sai no SaveChanges do UnitOfWorkBehavior.
        var comentario = await comentarios.ObterPorIdAsync(cmd.Id, ct)
                         ?? throw new NaoEncontradoException("Comentário", cmd.Id);

        comentario.Aprovado = cmd.Aprovado;

        // Respostas não mudam uma a uma: a listagem só devolve respostas DE raízes
        // visíveis, então esconder (ou reexibir) o pai leva a conversa junto.
        return Unit.Value;
    }
}
