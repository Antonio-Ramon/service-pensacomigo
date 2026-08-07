using MediatR;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Curtidas.Curtir;

public class CurtirPostCommandHandler(IPostRepository posts, ILikeRepository likes)
    : IRequestHandler<CurtirPostCommand, Unit>
{
    public async Task<Unit> Handle(CurtirPostCommand cmd, CancellationToken ct)
    {
        if (!await posts.ExistePorIdAsync(cmd.PostId, ct))
            throw new NaoEncontradoException("Post", cmd.PostId);

        // Idempotente: quem já curtiu clica de novo e recebe o mesmo 204, sem contar duas vezes.
        // Este `if` é conveniência, não garantia — entre ele e o INSERT cabe outra requisição.
        // Quem garante mesmo é o índice único (post_id, viewer_hash); a corrida vira 409.
        if (await likes.ExisteAsync(cmd.PostId, cmd.Visitante, ct))
            return Unit.Value;

        await likes.AdicionarAsync(new Like
        {
            Id = Guid.NewGuid(),
            PostId = cmd.PostId,
            ViewerHash = cmd.Visitante,
        }, ct);

        // ponytail: ExecuteUpdate grava JÁ, fora do commit do UnitOfWorkBehavior — se o INSERT
        // acima perder a corrida do índice único, o contador fica 1 acima. Janela de milissegundos
        // do mesmo visitante; se incomodar, envolver o behavior numa transação explícita.
        await posts.AjustarCurtidasAsync(cmd.PostId, +1, ct);

        return Unit.Value;
    }
}
