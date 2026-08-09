using MediatR;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Curtidas.Curtir;
using PensaComigo.Application.Curtidas.Descurtir;
using PensaComigo.Web.Visitantes;

namespace PensaComigo.Web.Controllers;

/// <summary>
/// Curtida é do post: rota aninhada, sem <c>[Authorize]</c> (leitor anônimo curte).
/// Sem rate limit também — a constraint única já limita a uma curtida por visitante por post.
/// </summary>
[ApiController]
[Route("api/v1/posts/{postId:guid}/curtidas")]
public class CurtidasController(ISender mediator, HashVisitante hash) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Curtir(Guid postId, CancellationToken ct)
    {
        await mediator.Send(new CurtirPostCommand(postId, hash.De(HttpContext)), ct);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Descurtir(Guid postId, CancellationToken ct)
    {
        await mediator.Send(new DescurtirPostCommand(postId, hash.De(HttpContext)), ct);

        return NoContent();
    }
}
