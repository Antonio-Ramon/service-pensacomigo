using MediatR;

namespace PensaComigo.Application.Posts;

/// <summary>Valor, pelo mesmo motivo de <c>CurtidasAtualizadas</c>: o GET do post incrementa.</summary>
public record PostVisualizado(Guid PostId, int Qtd) : INotification;

/// <summary>
/// Sinal, não valor — aqui o refetch é barato e honesto: a listagem é <c>[AllowAnonymous]</c>,
/// não incrementa nada, e o card do feed precisa de autor, tags e etapa que este evento não tem.
/// <para>
/// Sem grupo: quem está no feed não abriu post nenhum. Vai em <c>Clients.All</c>.
/// </para>
/// </summary>
public record PostPublicado(Guid PostId, string Slug) : INotification;

/// <summary>O <c>Slug</c> vai junto porque é por ele que o front acha o card na tela —
/// e quem estiver LENDO o post deletado precisa saber que ele sumiu.</summary>
public record PostRemovido(Guid PostId, string Slug) : INotification;
