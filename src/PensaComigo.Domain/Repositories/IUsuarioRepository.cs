using PensaComigo.Domain.Entities;

namespace PensaComigo.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Todos os autores, por nome — alimenta o "Quem escreve" público.</summary>
    Task<List<Usuario>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
}
