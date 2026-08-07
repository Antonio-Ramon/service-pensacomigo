using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Shared.Erros;

namespace PensaComigo.Web.Exceptions;

/// <summary>
/// Ponto ÚNICO onde exceção vira resposta de erro. Registrado via AddExceptionHandler,
/// disparado pelo app.UseExceptionHandler(). Controllers e handlers só lançam.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger, IHostEnvironment ambiente) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, mensagem) = Traduzir(ex);

        var resposta = new RespostaErro
        {
            Message = mensagem,
            Notifications = Notificar(ex),
            // Stack no corpo só em Development E só em 5xx: em 4xx o cliente já sabe o que
            // errou, e em produção isso vazaria caminho de arquivo, versão de lib e query.
            Debug = ambiente.IsDevelopment() && status >= StatusCodes.Status500InternalServerError
                ? ex.ToString()
                : null,
        };

        // 5xx é defeito nosso: log de erro com o contexto pra achar a requisição.
        // 4xx é o cliente errando: log informativo, senão o alerta vira ruído.
        var nivel = status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Information;
        logger.Log(nivel, ex, "{Status} em {Metodo} {Rota} (traceId {TraceId})",
            status, ctx.Request.Method, ctx.Request.Path, Activity.Current?.Id ?? ctx.TraceIdentifier);

        ctx.Response.StatusCode = status;

        // Escreve direto: delegar ao IProblemDetailsService permite que ele RECUSE o corpo
        // (retorna false) e o UseExceptionHandler sobrescreva tudo com a resposta genérica.
        await ctx.Response.WriteAsJsonAsync(resposta, options: null, contentType: "application/json", ct);

        return true;
    }

    private static (int Status, string Mensagem) Traduzir(Exception ex) => ex switch
    {
        NaoAutorizadoException => (StatusCodes.Status401Unauthorized, ex.Message),
        NaoEncontradoException => (StatusCodes.Status404NotFound, ex.Message),
        RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, ex.Message),

        ValidationException falhas when falhas.Errors.Any() =>
            (StatusCodes.Status422UnprocessableEntity,
             // A Message da ValidationException é o despejo "Validation failed: --" em inglês.
             string.Join(" ", falhas.Errors.Select(e => e.ErrorMessage).Distinct())),
        ValidationException => (StatusCodes.Status422UnprocessableEntity, "Um ou mais campos estão inválidos."),

        // Constraint do banco (FK em uso, índice único). Sem isto vira 500 — e não é defeito
        // nosso, é o cliente pedindo algo que o schema proíbe.
        DbUpdateException => (StatusCodes.Status409Conflict,
            "Não é possível concluir: este registro está em uso ou já existe."),

        _ => (StatusCodes.Status500InternalServerError, "Erro inesperado. Tente novamente."),
    };

    private static IReadOnlyList<Notificacao> Notificar(Exception ex) => ex switch
    {
        // Campo a campo, com a Key no mesmo camelCase do JSON que o cliente mandou.
        ValidationException falhas =>
            [.. falhas.Errors.Select(e => new Notificacao(
                JsonNamingPolicy.CamelCase.ConvertName(e.PropertyName), e.ErrorMessage))],

        // Erro sem campo culpado: uma notificação com a origem, igual ao escolaweb.
        NaoAutorizadoException or NaoEncontradoException or RegraDeNegocioException =>
            [new Notificacao("Erro", ex.Message)],

        DbUpdateException => [new Notificacao("Erro", "Registro em uso ou duplicado.")],

        _ => [new Notificacao("Erro", "Erro inesperado. Tente novamente.")],
    };
}
