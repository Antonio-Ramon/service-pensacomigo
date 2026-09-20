using Microsoft.AspNetCore.SignalR;

namespace PensaComigo.Web.Realtime;

/// <summary>
/// Hub único da aplicação. Nasceu como <c>ComentariosHub</c> e foi renomeado quando passou a
/// carregar curtida, visualização e feed (Fatia 25) — um hub por tipo de evento multiplicaria
/// conexões WebSocket por aba sem nenhum ganho.
/// <para>
/// Sem <c>[Authorize]</c>: comentar e curtir são anônimos por design (Fatias 20 e 22) —
/// exigir JWT tiraria o realtime justamente de quem o usa.
/// </para>
/// </summary>
public class TempoRealHub : Hub
{
    public Task Entrar(Guid postId) => Groups.AddToGroupAsync(Context.ConnectionId, Grupo(postId));

    public Task Sair(Guid postId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(postId));

    /// <summary>O grupo vive por ConnectionId: reconectar gera outro, e o cliente precisa
    /// reinvocar <c>Entrar</c> no <c>onreconnected</c>.
    /// <para>
    /// Post novo/deletado NÃO tem grupo: quem está no feed não abriu post nenhum, então
    /// esses dois eventos vão em <c>Clients.All</c>.
    /// </para></summary>
    public static string Grupo(Guid postId) => $"post:{postId}";
}
