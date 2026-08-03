using MediatR;
using PensaComigo.Application.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Tags.Criar;

/// <summary>
/// Regra do caso de uso: deriva o slug do nome, recusa slug duplicado (422 amigável
/// em vez de estourar o índice único no banco) e grava. UnitOfWorkBehavior commita.
/// </summary>
public class CriarTagCommandHandler(ITagRepository tags)
    : IRequestHandler<CriarTagCommand, TagResponse>
{
    public async Task<TagResponse> Handle(CriarTagCommand cmd, CancellationToken ct)
    {
        // Tag não resolve colisão com "-2": nome equivalente é a MESMA tag, então recusa.
        var slug = GeradorSlug.Gerar(cmd.Nome);

        if (await tags.ExistePorSlugAsync(slug, ct))
            throw new RegraDeNegocioException($"Já existe uma tag equivalente a \"{cmd.Nome}\".");

        var tag = new Tag { Id = Guid.NewGuid(), Nome = cmd.Nome.Trim(), Slug = slug };
        await tags.AdicionarAsync(tag, ct);

        return new TagResponse(tag.Id, tag.Nome, tag.Slug);
    }
}
