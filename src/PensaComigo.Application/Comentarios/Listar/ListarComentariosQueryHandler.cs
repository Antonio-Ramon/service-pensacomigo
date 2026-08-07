using MediatR;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>Sem regra: pede a página de raízes ao repo e projeta pra árvore rasa.</summary>
public class ListarComentariosQueryHandler(IComentarioRepository comentarios)
    : IRequestHandler<ListarComentariosQuery, Pagina<ComentarioListaResponse>>
{
    public async Task<Pagina<ComentarioListaResponse>> Handle(ListarComentariosQuery q, CancellationToken ct)
    {
        var pagina = await comentarios.ListarAprovadosAsync(q.PostId, q.Consulta, ct);

        return new Pagina<ComentarioListaResponse>(
            pagina.Items.Select(c => new ComentarioListaResponse(
                c.Id, c.Autor, c.Conteudo, c.DataCriacao,
                c.Respostas.Select(r => new RespostaResponse(r.Id, r.Autor, r.Conteudo, r.DataCriacao)).ToList()))
                .ToList(),
            pagina.TotalItems);
    }
}
