using MediatR;

namespace PensaComigo.Application.Curtidas;

/// <summary>
/// Carrega o <b>valor</b>, não um aviso: ao contrário do comentário, o cliente não pode
/// "refazer o GET" para descobrir o número — o GET do post é o <c>AbrirPostCommand</c>, que
/// <b>incrementa visualização</b>. Refetch a cada curtida inflaria o outro contador.
/// </summary>
public record CurtidasAtualizadas(Guid PostId, int Qtd) : INotification;
