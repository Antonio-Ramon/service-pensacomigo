using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Comentarios.Criar;

/// <summary>
/// Comentar. <paramref name="Visitante"/> é o hash calculado no host a partir de
/// IP + User-Agent: é a identidade do leitor anônimo pro rate limit.
/// <paramref name="UsuarioId"/> vem da claim <c>sub</c> quando há JWT — é o autor do
/// blog respondendo na conversa. Nunca do corpo, igual ao <c>AutorId</c> do post.
/// </summary>
public record CriarComentarioCommand(
    Guid PostId,
    Guid? ParentId,
    string Autor,
    string Conteudo,
    string Visitante,
    Guid? UsuarioId = null) : ICommand<ComentarioResponse>;
