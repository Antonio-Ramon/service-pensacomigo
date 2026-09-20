using Microsoft.AspNetCore.SignalR;

namespace PensaComigo.Web.Realtime;

/// <summary>
/// Hub dos comentários. Sem <c>[Authorize]</c>: comentar e curtir são anônimos por design
/// (Fatias 20 e 22) — exigir JWT tiraria o realtime justamente de quem o usa.
/// </summary>
public class ComentariosHub : Hub
{
    public Task Entrar(Guid postId) => Groups.AddToGroupAsync(Context.ConnectionId, Grupo(postId));

    public Task Sair(Guid postId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(postId));

    /// <summary>O grupo vive por ConnectionId: reconectar gera outro, e o cliente precisa
    /// reinvocar <c>Entrar</c> no <c>onreconnected</c>.</summary>
    public static string Grupo(Guid postId) => $"post:{postId}";
}
