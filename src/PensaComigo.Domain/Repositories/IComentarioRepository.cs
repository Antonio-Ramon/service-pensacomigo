using Gridify;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IComentarioRepository
{
    Task<Comentario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Página de comentários RAIZ do post, cada um já com suas respostas.
    /// Só aprovados, salvo <paramref name="incluirOcultos"/> (moderação).
    /// O que é filtrável pela querystring é whitelist do repo.</summary>
    Task<Pagina<Comentario>> ListarAsync(
        Guid postId, IGridifyQuery consulta, bool incluirOcultos = false, CancellationToken ct = default);

    Task AdicionarAsync(Comentario comentario, CancellationToken ct = default);

    void Remover(Comentario comentario);
}
