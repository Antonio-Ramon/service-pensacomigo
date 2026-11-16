using Gridify;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.UnitTests.Fakes;

/// <summary>
/// Só o que os casos de uso testados aqui tocam; o resto da interface estoura se for chamado —
/// um teste que caia em <c>NotSupportedException</c> está exercitando outro caminho que o esperado.
/// </summary>
public sealed class PostRepositorioFake(int? contadorCurtidas = null, Post? post = null) : IPostRepository
{
    public Post? Removido { get; private set; }

    /// <summary>O que o <c>UPDATE ... RETURNING</c> devolveria; <c>null</c> = nenhuma linha casou.</summary>
    public Task<int?> AjustarCurtidasAsync(Guid id, int delta, CancellationToken ct = default) =>
        Task.FromResult(contadorCurtidas);

    public Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(true);

    public Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(post);

    public void Remover(Post p) => Removido = p;

    public Task<Guid?> ObterAutorIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Pagina<Post>> ListarAsync(IGridifyQuery consulta, bool incluirRascunhos = false, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Post?> ObterPorSlugAsync(string slug, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Post?> ObterDetalhePorIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task IncrementarVisualizacoesAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<string>> ListarSlugsComPrefixoAsync(string prefixo, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Post?> ObterParaEdicaoAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task AdicionarAsync(Post p, CancellationToken ct = default) => throw new NotSupportedException();
}
