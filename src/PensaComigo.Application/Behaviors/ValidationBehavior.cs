using FluentValidation;
using MediatR;

namespace PensaComigo.Application.Behaviors;

/// <summary>
/// Roda todos os validators FluentValidation registrados para o request.
/// Falhou → lança ValidationException (traduzida a 422 pelo middleware da Fatia 6).
/// Sem validator registrado, passa direto.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (validators.Any())
        {
            var contexto = new ValidationContext<TRequest>(request);
            var falhas = validators
                .Select(v => v.Validate(contexto))
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (falhas.Count != 0)
                throw new ValidationException(falhas);
        }

        return await next(ct);
    }
}
