using PensaComigo.Application.Messaging;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Editar;

public record EditarPostCommand(
    Guid Id,
    Guid AutorId,
    string Titulo,
    string ImagemCapa,
    List<Guid> TagIds,
    List<Bloco> Conteudo) : ICommand<PostResponse>, IPostEscrita;
