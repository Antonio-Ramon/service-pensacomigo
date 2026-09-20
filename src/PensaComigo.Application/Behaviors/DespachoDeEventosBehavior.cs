using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Behaviors;

/// <summary>
/// Envolve o <c>UnitOfWorkBehavior</c>: drena a fila SÓ depois que o <c>CommitAsync</c> voltou.
/// Registrado ANTES dele em <c>AddOpenBehavior</c> — inverter as duas linhas volta a anunciar
/// linha que nenhum INSERT levou ao Postgres.
/// </summary>
/// <remarks>
/// Sem try/catch de propósito: commit que estoura sobe a exceção e o foreach nunca é alcançado.
/// A garantia "evento só depois do commit" é a ausência de tratamento, não um if.
/// </remarks>
public class DespachoDeEventosBehavior<TRequest, TResponse>(FilaDeEventos fila, IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var resposta = await next(ct);

        foreach (var evento in fila.Drenar())
            await publisher.Publish(evento, ct);

        return resposta;
    }
}
