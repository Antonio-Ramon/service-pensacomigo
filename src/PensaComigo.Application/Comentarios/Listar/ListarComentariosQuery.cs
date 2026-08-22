using Gridify;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Common;

namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>
/// Leitura pura → <see cref="IQuery{T}"/> (sem commit).
/// <para>
/// Aqui a query <b>NÃO herda</b> de <see cref="GridifyQuery"/>, ao contrário de Tags
/// (Fatia 13) e do feed (Fatia 19): quem é bindado da querystring é a
/// <see cref="Consulta"/>, e o <see cref="PostId"/> vem da rota. Herdando, o
/// <c>PostId</c> viraria propriedade bindável e apareceria no Swagger como
/// <c>?postId=</c> — um parâmetro que o cliente não deve mandar e que o servidor
/// ignoraria. Composição separa "o que o cliente pede" de "o que o servidor decide".
/// </para>
/// </summary>
public class ListarComentariosQuery(Guid postId, IGridifyQuery consulta)
    : IQuery<Pagina<ComentarioListaResponse>>
{
    public Guid PostId { get; } = postId;

    public IGridifyQuery Consulta { get; } = consulta;

    /// <summary>Ocultos só para admin. Quem decide é o controller, pela claim — nunca o cliente.</summary>
    public bool IncluirOcultos { get; init; }
}
