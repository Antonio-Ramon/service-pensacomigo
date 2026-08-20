using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Editar;

public record EditarPostCommand(
    Guid Id,
    Guid AutorId,
    string Titulo,
    string? Dek,
    string ImagemCapa,
    List<Guid> TagIds,
    List<Bloco> Conteudo,
    StatusPost Status,
    List<Mood> Moods,
    Guid? EtapaId,
    DateTime? DataPublicacao) : ICommand<PostResponse>, IPostEscrita;
