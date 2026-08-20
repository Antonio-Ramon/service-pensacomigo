using Gridify;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Common;

namespace PensaComigo.Application.Posts.Listar;

/// <summary>
/// Feed público (leitura pura → Query, sem commit). Mesmo padrão da listagem de tags
/// (Fatia 13): Page/PageSize/OrderBy/Filter vêm da querystring via <see cref="GridifyQuery"/>.
/// Classe, não record — record não herda de classe comum.
/// </summary>
public class ListarPostsQuery : GridifyQuery, IQuery<Pagina<PostResumoResponse>>
{
    /// <summary>Rascunhos só para autor logado. O controller SEMPRE sobrescreve depois do
    /// binding — o valor que o cliente mandar na querystring é ignorado.</summary>
    public bool IncluirRascunhos { get; set; }
}
