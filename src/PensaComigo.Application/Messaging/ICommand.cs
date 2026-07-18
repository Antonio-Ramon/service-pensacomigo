using MediatR;

namespace PensaComigo.Application.Messaging;

/// <summary>Marcador não-genérico: permite ao UnitOfWorkBehavior testar `is IBaseCommand`.</summary>
public interface IBaseCommand;

/// <summary>Escrita (CQRS). Passa pelo UnitOfWork → commita ao fim.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand;

/// <summary>Leitura (CQRS). Não commita.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;
