using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Criar;

/// <summary>
/// O corpo do POST. Separado do Command porque o <c>AutorId</c> NÃO vem do cliente:
/// sai da claim `sub` no controller (mesmo motivo da Fatia 14 — quem manda o dono
/// do recurso posta em nome dos outros).
/// </summary>
public record CriarPostRequest(
    string Titulo,
    string ImagemCapa,
    List<Guid> TagIds,
    List<Bloco> Conteudo,
    StatusPost Status = StatusPost.Publicado);   // default preserva o contrato antigo: POST publica
