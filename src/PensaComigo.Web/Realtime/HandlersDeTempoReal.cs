using MediatR;
using Microsoft.AspNetCore.SignalR;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Curtidas;
using PensaComigo.Application.Posts;

namespace PensaComigo.Web.Realtime;

/// <summary>
/// A ponte evento → WebSocket. Mora no host porque <c>IHubContext</c> é ASP.NET Core — a
/// Application não o conhece. Por isso nenhum destes é achado pelo scan do MediatR (que varre a
/// Application): cada um é registrado à mão no <c>Program.cs</c>, e <c>Publish</c> sem handler
/// não reclama.
/// <para>
/// Os dois primeiros são <b>do post</b> e vão para o grupo <c>post:{id}</c>. Os dois últimos
/// são <b>do feed</b>: quem está na home não abriu post nenhum e portanto não está em grupo
/// algum — vão para <c>Clients.All</c>.
/// </para>
/// </summary>
public class ComentarioCriadoHandler(IHubContext<TempoRealHub> hub)
    : INotificationHandler<ComentarioCriado>
{
    public Task Handle(ComentarioCriado evento, CancellationToken ct) =>
        hub.Clients.Group(TempoRealHub.Grupo(evento.Comentario.PostId))
                   .SendAsync("ComentarioCriado", evento.Comentario, ct);
}

public class CurtidasAtualizadasHandler(IHubContext<TempoRealHub> hub)
    : INotificationHandler<CurtidasAtualizadas>
{
    public Task Handle(CurtidasAtualizadas evento, CancellationToken ct) =>
        hub.Clients.Group(TempoRealHub.Grupo(evento.PostId))
                   .SendAsync("CurtidasAtualizadas", evento.Qtd, ct);
}

public class PostVisualizadoHandler(IHubContext<TempoRealHub> hub)
    : INotificationHandler<PostVisualizado>
{
    public Task Handle(PostVisualizado evento, CancellationToken ct) =>
        hub.Clients.Group(TempoRealHub.Grupo(evento.PostId))
                   .SendAsync("PostVisualizado", evento.Qtd, ct);
}

public class PostPublicadoHandler(IHubContext<TempoRealHub> hub)
    : INotificationHandler<PostPublicado>
{
    public Task Handle(PostPublicado evento, CancellationToken ct) =>
        hub.Clients.All.SendAsync("PostPublicado", evento.Slug, ct);
}

public class PostRemovidoHandler(IHubContext<TempoRealHub> hub)
    : INotificationHandler<PostRemovido>
{
    public Task Handle(PostRemovido evento, CancellationToken ct) =>
        hub.Clients.All.SendAsync("PostRemovido", evento.Slug, ct);
}
