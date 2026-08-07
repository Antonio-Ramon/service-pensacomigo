using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Comentarios.Moderar;

/// <summary>
/// Esconder = <c>aprovado = false</c>. Não apaga: o texto continua no banco para
/// eventual auditoria, só some da listagem pública (que filtra por aprovado).
/// Quem pode chamar é decidido no controller pela claim <c>is_admin</c>.
/// </summary>
public record OcultarComentarioCommand(Guid Id) : ICommand<Unit>;
