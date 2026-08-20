using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Tags.Deletar;

public record DeletarTagCommand(Guid Id) : ICommand<MediatR.Unit>;
