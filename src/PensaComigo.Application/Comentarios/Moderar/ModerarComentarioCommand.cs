using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Comentarios.Moderar;

/// <summary>
/// Liga/desliga a visibilidade do comentário (<c>aprovado</c>). Não apaga: o texto continua
/// no banco, só some da listagem pública — e por isso ocultar é reversível (<c>Aprovado=true</c>).
/// Quem pode chamar é decidido no controller pela claim <c>is_admin</c>.
/// </summary>
public record ModerarComentarioCommand(Guid Id, bool Aprovado) : ICommand<Unit>;
