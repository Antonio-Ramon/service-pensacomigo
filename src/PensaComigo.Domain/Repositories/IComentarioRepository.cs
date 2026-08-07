using Gridify;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IComentarioRepository
{
    Task<Comentario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Página de comentários RAIZ aprovados do post, cada um já com suas
    /// respostas aprovadas. O que é filtrável pela querystring é whitelist do repo.</summary>
    Task<Pagina<Comentario>> ListarAprovadosAsync(Guid postId, IGridifyQuery consulta, CancellationToken ct = default);

    Task AdicionarAsync(Comentario comentario, CancellationToken ct = default);

    void Remover(Comentario comentario);
}
