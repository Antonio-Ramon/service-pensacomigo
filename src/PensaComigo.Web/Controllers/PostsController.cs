using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Posts;
using PensaComigo.Application.Posts.Criar;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostsController(ISender mediator) : ControllerBase
{
    /// <summary>Só autor autenticado publica, e sempre em nome de si mesmo: o autor
    /// sai da claim `sub`, nunca do corpo.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostResponse>> Criar(CriarPostRequest req, CancellationToken ct)
    {
        var autorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var command = new CriarPostCommand(autorId, req.Titulo, req.ImagemCapa, req.TagIds, req.Conteudo);

        return Ok(await mediator.Send(command, ct));
    }
}
