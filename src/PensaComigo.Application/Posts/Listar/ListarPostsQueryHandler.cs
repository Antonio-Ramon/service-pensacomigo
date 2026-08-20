using MediatR;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Listar;

/// <summary>Sem regra: repassa a consulta ao repo e projeta pro card do feed.</summary>
public class ListarPostsQueryHandler(IPostRepository posts)
    : IRequestHandler<ListarPostsQuery, Pagina<PostResumoResponse>>
{
    public async Task<Pagina<PostResumoResponse>> Handle(ListarPostsQuery q, CancellationToken ct)
    {
        var pagina = await posts.ListarAsync(q, q.IncluirRascunhos, ct);

        return new Pagina<PostResumoResponse>(
            pagina.Items.Select(p => new PostResumoResponse(
                p.Id, p.Titulo, p.Dek, p.Slug, p.ImagemCapa,
                p.TempoLeitura, p.QtdCurtidas, p.QtdVisualizacoes, p.DataCriacao,
                new AutorResponse(p.Autor.Id, p.Autor.Nome, p.Autor.ImagemUrl),
                p.Tags.Select(t => new TagResponse(t.Id, t.Nome, t.Slug)).ToList(),
                p.Status, p.DataPublicacao,
                p.Moods,
                p.Etapa is null ? null : Etapas.EtapaResponse.De(p.Etapa))).ToList(),
            pagina.TotalItems);
    }
}
