using MediatR;
using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Posts.Deletar;

/// <summary><c>Unit</c> = o "void" do MediatR: não há resposta, mas todo request tem um tipo de retorno.</summary>
public record DeletarPostCommand(Guid Id, Guid AutorId) : ICommand<Unit>;
