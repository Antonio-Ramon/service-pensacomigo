using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Tags.Deletar;

/// <summary>Tag vinculada a post não sai (issue #32): bloqueia com 422 informando a
/// contagem, em vez de desvincular em cascata por baixo dos panos.</summary>
public class DeletarTagCommandHandler(ITagRepository tags)
    : IRequestHandler<DeletarTagCommand, Unit>
{
    public async Task<Unit> Handle(DeletarTagCommand cmd, CancellationToken ct)
    {
        var tag = await tags.ObterPorIdAsync(cmd.Id, ct)
            ?? throw new NaoEncontradoException("Tag", cmd.Id.ToString());

        var emUso = await tags.ContarPostsAsync(cmd.Id, ct);
        if (emUso > 0)
            throw new RegraDeNegocioException(
                $"A tag '{tag.Nome}' está vinculada a {emUso} post(s). Desvincule antes de excluir.");

        tags.Remover(tag);
        return Unit.Value;
    }
}
