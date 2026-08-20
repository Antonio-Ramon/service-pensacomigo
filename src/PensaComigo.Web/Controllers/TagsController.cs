using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Tags;
using PensaComigo.Application.Tags.Criar;
using PensaComigo.Application.Tags.Deletar;
using PensaComigo.Application.Tags.Listar;
using PensaComigo.Domain.Common;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TagsController(ISender mediator) : ControllerBase
{
    /// <summary>Público: qualquer um lista as tags pra navegar por tema. Sem token.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<Pagina<TagResponse>>> Listar(
        [FromQuery] ListarTagsQuery query, CancellationToken ct) =>
        Ok(await mediator.Send(query, ct));

    /// <summary>Protegido: só autor autenticado cria tag. Sem token → 401 automático.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TagResponse>> Criar(CriarTagCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    /// <summary>Excluir tag (issue #32). Vinculada a post → 422 com a contagem.</summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeletarTagCommand(id), ct);

        return NoContent();
    }
}
