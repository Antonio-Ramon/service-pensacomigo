using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface ILikeRepository
{
    /// <summary>Só a existência (vira `EXISTS`) — o caminho comum de curtir não precisa da linha.</summary>
    Task<bool> ExisteAsync(Guid postId, string viewerHash, CancellationToken ct = default);

    /// <summary>A curtida RASTREADA, para o descurtir conseguir removê-la.</summary>
    Task<Like?> ObterAsync(Guid postId, string viewerHash, CancellationToken ct = default);

    Task AdicionarAsync(Like like, CancellationToken ct = default);

    void Remover(Like like);
}
