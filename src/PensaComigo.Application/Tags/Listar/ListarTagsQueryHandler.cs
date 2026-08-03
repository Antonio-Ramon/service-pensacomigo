using MediatR;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Tags.Listar;

/// <summary>Só lê. Sem regra: repassa a consulta (filtro/ordem/página) ao repo e projeta pro response.</summary>
public class ListarTagsQueryHandler(ITagRepository tags)
    : IRequestHandler<ListarTagsQuery, Pagina<TagResponse>>
{
    public async Task<Pagina<TagResponse>> Handle(ListarTagsQuery q, CancellationToken ct)
    {
        var pagina = await tags.ListarAsync(q, ct);
        return new Pagina<TagResponse>(
            pagina.Items.Select(t => new TagResponse(t.Id, t.Nome, t.Slug)).ToList(),
            pagina.TotalItems);
    }
}
