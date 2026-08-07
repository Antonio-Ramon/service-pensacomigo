using Gridify;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Pagina<Tag>> ListarAsync(IGridifyQuery consulta, CancellationToken ct = default);
    Task<bool> ExistePorSlugAsync(string slug, CancellationToken ct = default);

    /// <summary>As entidades RASTREADAS das tags pedidas — é o que o EF precisa para
    /// gravar só as linhas de junção do N:N, sem tentar reinserir a tag.</summary>
    Task<List<Tag>> ObterPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AdicionarAsync(Tag tag, CancellationToken ct = default);
}
