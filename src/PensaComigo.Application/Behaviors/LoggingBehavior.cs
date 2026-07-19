using MediatR;
using Microsoft.Extensions.Logging;

namespace PensaComigo.Application.Behaviors;

/// <summary>Behavior mais externo: registra início e fim de todo request.</summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var nome = typeof(TRequest).Name;
        logger.LogInformation("Tratando {Request}", nome);
        var resposta = await next(ct);
        logger.LogInformation("Tratado {Request}", nome);
        return resposta;
    }
}
