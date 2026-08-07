using Gridify;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Common;

namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>
/// Leitura pura → <see cref="IQuery{T}"/> (sem commit). Page/PageSize/OrderBy/Filter
/// vêm da querystring pelo <see cref="GridifyQuery"/>, como em Tags (Fatia 13) e no
/// feed (Fatia 19). Classe, não record — record não herda de classe comum.
/// <para>
/// <see cref="PostId"/> é preenchido pelo controller a partir da ROTA, depois do
/// model binding: mesmo que o cliente mande <c>?postId=</c>, o valor da rota vence.
/// </para>
/// </summary>
public class ListarComentariosQuery : GridifyQuery, IQuery<Pagina<ComentarioListaResponse>>
{
    public Guid PostId { get; set; }
}
