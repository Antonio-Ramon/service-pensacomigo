using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Links;
using PensaComigo.Application.Links.Preview;

namespace PensaComigo.Web.Controllers;

/// <summary>Ferramenta do editor, não rota pública — daí o [Authorize].</summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class LinksController(ISender mediator) : ControllerBase
{
    /// <summary>Preview Open Graph pro bloco de link: colar URL no editor monta o card.</summary>
    [HttpGet("preview")]
    public async Task<ActionResult<LinkPreviewResponse>> Preview(
        [FromQuery] string url, CancellationToken ct) =>
        Ok(await mediator.Send(new ObterPreviewLinkQuery(url), ct));
}
