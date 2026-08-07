using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Curtidas.Descurtir;

public record DescurtirPostCommand(Guid PostId, string Visitante) : ICommand<Unit>;
