using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Slugs já usados que começam por <paramref name="prefixo"/> — insumo do
    /// <c>GeradorSlug.ResolverColisao</c>. Uma ida ao banco em vez de N.</summary>
    Task<IReadOnlyList<string>> ListarSlugsComPrefixoAsync(string prefixo, CancellationToken ct = default);

    Task AdicionarAsync(Post post, CancellationToken ct = default);
}
