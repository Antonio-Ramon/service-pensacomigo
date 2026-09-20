using MediatR;
using Microsoft.AspNetCore.SignalR;
using PensaComigo.Application.Comentarios;

namespace PensaComigo.Web.Realtime;

/// <summary>
/// Mora no host porque <c>IHubContext</c> é ASP.NET Core — a Application não o conhece.
/// Por isso não é achado pelo scan do MediatR (que varre a Application) e é registrado
/// à mão no <c>Program.cs</c>.
/// </summary>
public class ComentarioCriadoHandler(IHubContext<ComentariosHub> hub)
    : INotificationHandler<ComentarioCriado>
{
    public Task Handle(ComentarioCriado evento, CancellationToken ct) =>
        hub.Clients.Group(ComentariosHub.Grupo(evento.Comentario.PostId))
                   .SendAsync("ComentarioCriado", evento.Comentario, ct);
}
