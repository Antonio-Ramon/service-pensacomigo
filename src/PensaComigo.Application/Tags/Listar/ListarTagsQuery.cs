using Gridify;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Common;

namespace PensaComigo.Application.Tags.Listar;

/// <summary>
/// Lista pública de tags (leitura pura → Query, sem commit).
/// Herda <see cref="GridifyQuery"/>: Page/PageSize/OrderBy/Filter vêm da querystring
/// e aparecem sozinhos no Swagger. Classe (não record) porque record não herda de classe comum.
/// </summary>
public class ListarTagsQuery : GridifyQuery, IQuery<Pagina<TagResponse>>;
