using MediatR;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Curtidas.Descurtir;

public class DescurtirPostCommandHandler(IPostRepository posts, ILikeRepository likes)
    : IRequestHandler<DescurtirPostCommand, Unit>
{
    public async Task<Unit> Handle(DescurtirPostCommand cmd, CancellationToken ct)
    {
        var like = await likes.ObterAsync(cmd.PostId, cmd.Visitante, ct);

        // Não curtia (ou o post nem existe): o estado que o cliente pediu já vale. 204, sem 404 —
        // um DELETE repetido pela rede não pode virar erro.
        if (like is null)
            return Unit.Value;

        likes.Remover(like);
        await posts.AjustarCurtidasAsync(cmd.PostId, -1, ct);

        return Unit.Value;
    }
}
