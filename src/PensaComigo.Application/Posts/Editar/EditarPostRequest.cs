using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Editar;

/// <summary>
/// Corpo do PUT. Sem <c>Id</c> (vem da rota), sem <c>AutorId</c> (vem da claim `sub`)
/// e sem <c>Slug</c>: o slug congela na criação — quem manda o slug renomeia a URL
/// de um post já compartilhado.
/// </summary>
public record EditarPostRequest(
    string Titulo,
    string ImagemCapa,
    List<Guid> TagIds,
    List<Bloco> Conteudo,
    StatusPost Status = StatusPost.Publicado,   // default preserva o contrato antigo
    string? Dek = null,
    List<Mood>? Moods = null,
    Guid? EtapaId = null,
    DateTime? DataPublicacao = null);           // só usada com Status=Agendado
