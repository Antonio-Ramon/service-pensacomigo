using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts;

/// <summary>
/// O que criar e editar têm em comum. Existe só para as regras de validação
/// serem escritas UMA vez (<see cref="PostEscritaValidator{T}"/>).
/// </summary>
public interface IPostEscrita
{
    string Titulo { get; }
    string? Dek { get; }
    string ImagemCapa { get; }
    List<Guid> TagIds { get; }
    List<Bloco> Conteudo { get; }
    StatusPost Status { get; }
    List<Mood> Moods { get; }
    Guid? EtapaId { get; }

    /// <summary>Só faz sentido com <see cref="StatusPost.Agendado"/>: quando o post entra no ar.</summary>
    DateTime? DataPublicacao { get; }
}
