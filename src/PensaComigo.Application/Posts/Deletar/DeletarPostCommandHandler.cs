using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Deletar;

/// <summary>
/// Delete físico. Comentários, likes e as linhas de post_tags caem junto por
/// <c>OnDelete(Cascade)</c> no schema — o banco resolve, não o handler.
/// </summary>
public class DeletarPostCommandHandler(IPostRepository posts)
    : IRequestHandler<DeletarPostCommand, Unit>
{
    public async Task<Unit> Handle(DeletarPostCommand cmd, CancellationToken ct)
    {
        var post = await posts.ObterPorIdAsync(cmd.Id, ct);

        if (post is null || post.AutorId != cmd.AutorId)
            throw new NaoEncontradoException("Post", cmd.Id.ToString());

        posts.Remover(post);

        return Unit.Value;
    }
}
