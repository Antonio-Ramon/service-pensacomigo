using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Tags.Criar;

/// <summary>
/// Criar tag. Command (grava → commita). Só o <c>Nome</c> vem do cliente; o slug
/// é derivado no handler (campo calculado). Quem cria precisa estar autenticado —
/// isso é o [Authorize] no controller, não um campo aqui.
/// </summary>
public record CriarTagCommand(string Nome) : ICommand<TagResponse>;
