using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Comentarios.Moderar;

/// <summary>Delete físico. As respostas caem junto por <c>OnDelete(Cascade)</c>
/// na auto-referência <c>parent_id</c> — o banco resolve, não o handler.</summary>
public record DeletarComentarioCommand(Guid Id) : ICommand<Unit>;
