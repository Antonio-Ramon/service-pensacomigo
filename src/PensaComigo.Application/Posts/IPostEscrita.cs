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
    string ImagemCapa { get; }
    List<Guid> TagIds { get; }
    List<Bloco> Conteudo { get; }
    StatusPost Status { get; }
}
