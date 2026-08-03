using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MediatR;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Tags.Criar;

/// <summary>
/// Regra do caso de uso: deriva o slug do nome, recusa slug duplicado (422 amigável
/// em vez de estourar o índice único no banco) e grava. UnitOfWorkBehavior commita.
/// </summary>
public partial class CriarTagCommandHandler(ITagRepository tags)
    : IRequestHandler<CriarTagCommand, TagResponse>
{
    public async Task<TagResponse> Handle(CriarTagCommand cmd, CancellationToken ct)
    {
        var slug = GerarSlug(cmd.Nome);

        if (await tags.ExistePorSlugAsync(slug, ct))
            throw new RegraDeNegocioException($"Já existe uma tag equivalente a \"{cmd.Nome}\".");

        var tag = new Tag { Id = Guid.NewGuid(), Nome = cmd.Nome.Trim(), Slug = slug };
        await tags.AdicionarAsync(tag, ct);

        return new TagResponse(tag.Id, tag.Nome, tag.Slug);
    }

    // Campo calculado (como TempoLeitura): vive na Application, não na entidade.
    // "Saúde Mental" → "saude-mental". Tira acento, minúscula, troca não-alfanumérico por hífen.
    private static string GerarSlug(string nome)
    {
        var decomposto = nome.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var semAcento = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(c);

        return NaoAlfanumerico().Replace(semAcento.ToString(), "-").Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NaoAlfanumerico();
}
