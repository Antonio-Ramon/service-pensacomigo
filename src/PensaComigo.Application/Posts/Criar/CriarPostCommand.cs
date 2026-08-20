using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Criar;

/// <summary>
/// Criar post. Command → o UnitOfWorkBehavior (Fatia 5) commita ao fim.
/// Slug e tempo de leitura não estão aqui: são derivados no handler.
/// </summary>
public record CriarPostCommand(
    Guid AutorId,
    string Titulo,
    string ImagemCapa,
    List<Guid> TagIds,
    List<Bloco> Conteudo,
    StatusPost Status) : ICommand<PostResponse>, IPostEscrita;
