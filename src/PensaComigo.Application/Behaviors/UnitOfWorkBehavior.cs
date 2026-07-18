using MediatR;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Behaviors;

/// <summary>
/// Behavior mais interno: commita a transação SÓ em Commands, e SÓ depois que
/// o handler rodou com sucesso. Query (não é IBaseCommand) passa sem SaveChanges.
/// </summary>
public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IBaseCommand)
            return await next(ct);

        var resposta = await next(ct);
        await uow.CommitAsync(ct);
        return resposta;
    }
}
