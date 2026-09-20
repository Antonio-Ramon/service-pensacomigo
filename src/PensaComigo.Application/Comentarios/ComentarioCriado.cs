using MediatR;

namespace PensaComigo.Application.Comentarios;

/// <summary>
/// Já aconteceu — daí o tempo passado. Quem escuta não tem veto: só nasce pós-commit.
/// Quem empurra para o WebSocket é o <c>ComentarioCriadoHandler</c>, no host.
/// </summary>
public record ComentarioCriado(ComentarioResponse Comentario) : INotification;
