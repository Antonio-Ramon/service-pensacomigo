using Gridify;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Id do dono do post, sem trazer a entidade (e o `conteudo` jsonb) junto.
    /// null quando o post não existe.</summary>
    Task<Guid?> ObterAutorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Só a existência (vira `EXISTS` no SQL) — para quem não vai usar a entidade.</summary>
    Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Feed paginado/filtrado/ordenado pela querystring (Gridify, Fatia 13).</summary>
    Task<Pagina<Post>> ListarAsync(IGridifyQuery consulta, bool incluirRascunhos = false, CancellationToken ct = default);

    /// <summary>Post para leitura pública: sem tracking, com Autor e Tags.</summary>
    Task<Post?> ObterPorSlugAsync(string slug, CancellationToken ct = default);

    /// <summary>Detalhe por id para o editor do autor: sem tracking, com Autor, Tags e Etapa.
    /// Não incrementa visualizações — esse é o motivo de existir (issue #29).</summary>
    Task<Post?> ObterDetalhePorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Incremento atômico do contador de visualizações.</summary>
    Task IncrementarVisualizacoesAsync(Guid id, CancellationToken ct = default);

    /// <summary>Soma <paramref name="delta"/> (+1 ao curtir, -1 ao descurtir) no contador
    /// desnormalizado, de forma atômica e sem deixá-lo ficar negativo.</summary>
    Task AjustarCurtidasAsync(Guid id, int delta, CancellationToken ct = default);

    /// <summary>Slugs já usados que começam por <paramref name="prefixo"/> — insumo do
    /// <c>GeradorSlug.ResolverColisao</c>. Uma ida ao banco em vez de N.</summary>
    Task<IReadOnlyList<string>> ListarSlugsComPrefixoAsync(string prefixo, CancellationToken ct = default);

    /// <summary>Post RASTREADO e com as Tags carregadas — é assim que o change tracker
    /// consegue emitir o UPDATE e o delta da junção. Não use para leitura.</summary>
    Task<Post?> ObterParaEdicaoAsync(Guid id, CancellationToken ct = default);

    Task AdicionarAsync(Post post, CancellationToken ct = default);

    void Remover(Post post);
}
