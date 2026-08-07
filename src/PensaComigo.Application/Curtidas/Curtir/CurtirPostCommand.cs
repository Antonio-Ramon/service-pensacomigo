using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Curtidas.Curtir;

/// <summary>
/// Curtir é anônimo. <paramref name="Visitante"/> é o hash calculado no host (IP + User-Agent),
/// o mesmo da Fatia 20 — só que aqui ele vira chave de unicidade no banco, não balde em memória.
/// </summary>
public record CurtirPostCommand(Guid PostId, string Visitante) : ICommand<Unit>;
