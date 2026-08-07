using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Posts.Abrir;

/// <summary>
/// Abrir um post LÊ o post e ESCREVE o contador de visualizações. Pelo critério firmado
/// na Fatia 17 (efeito colateral, não "escreve no banco"), isso é <see cref="ICommand{T}"/>
/// — mesmo o GET sendo público e anônimo. Verbo HTTP e marcador CQRS são coisas diferentes.
/// </summary>
public record AbrirPostCommand(string Slug) : ICommand<PostDetalheResponse>;
