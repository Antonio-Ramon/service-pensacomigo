using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Comentarios.Criar;

/// <summary>
/// Comentar. <paramref name="Visitante"/> é o hash calculado no host a partir de
/// IP + User-Agent: é a identidade do leitor anônimo pro rate limit.
/// </summary>
public record CriarComentarioCommand(
    Guid PostId,
    Guid? ParentId,
    string Autor,
    string Conteudo,
    string Visitante) : ICommand<ComentarioResponse>;
