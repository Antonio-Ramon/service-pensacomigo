using MediatR;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Curtidas.Descurtir;

public class DescurtirPostCommandHandler(IPostRepository posts, ILikeRepository likes, FilaDeEventos eventos)
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
        var qtd = await posts.AjustarCurtidasAsync(cmd.PostId, -1, ct);

        if (qtd is int total)
            eventos.Adicionar(new CurtidasAtualizadas(cmd.PostId, total));

        return Unit.Value;
    }
}
