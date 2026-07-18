using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Web.Exceptions;

/// <summary>
/// Traduz exceção tipada → ProblemDetails (RFC 7807). Registrado via AddExceptionHandler;
/// disparado pelo app.UseExceptionHandler(). Controllers ficam magros: só lançam.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validação falhou"),
            RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno"),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Exceção não tratada");

        ctx.Response.StatusCode = status;

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = status == StatusCodes.Status500InternalServerError ? null : ex.Message,
        };

        if (ex is ValidationException falhas)
            problema.Extensions["erros"] = falhas.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = ctx,
            Exception = ex,
            ProblemDetails = problema,
        });
    }
}
