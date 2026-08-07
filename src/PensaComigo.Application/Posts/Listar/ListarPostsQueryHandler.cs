using MediatR;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Listar;

/// <summary>Sem regra: repassa a consulta ao repo e projeta pro card do feed.</summary>
public class ListarPostsQueryHandler(IPostRepository posts)
    : IRequestHandler<ListarPostsQuery, Pagina<PostResumoResponse>>
{
    public async Task<Pagina<PostResumoResponse>> Handle(ListarPostsQuery q, CancellationToken ct)
    {
        var pagina = await posts.ListarAsync(q, ct);

        return new Pagina<PostResumoResponse>(
            pagina.Items.Select(p => new PostResumoResponse(
                p.Id, p.Titulo, p.Slug, p.ImagemCapa,
                p.TempoLeitura, p.QtdCurtidas, p.QtdVisualizacoes, p.DataCriacao)).ToList(),
            pagina.TotalItems);
    }
}
