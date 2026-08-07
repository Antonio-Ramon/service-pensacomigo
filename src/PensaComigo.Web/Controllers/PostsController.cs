using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Posts;
using PensaComigo.Application.Posts.Abrir;
using PensaComigo.Application.Posts.Criar;
using PensaComigo.Application.Posts.Deletar;
using PensaComigo.Application.Posts.Editar;
using PensaComigo.Application.Posts.Listar;
using PensaComigo.Domain.Common;

namespace PensaComigo.Web.Controllers;

/// <summary>Escrita de post é sempre em nome de si mesmo: o autor sai da claim `sub`,
/// nunca do corpo. Por isso o controller inteiro é <c>[Authorize]</c>.</summary>
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class PostsController(ISender mediator) : ControllerBase
{
    /// <summary>Feed público. Filtro/ordem/página na querystring (Gridify).</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<Pagina<PostResumoResponse>>> Listar(
        [FromQuery] ListarPostsQuery query, CancellationToken ct) =>
        Ok(await mediator.Send(query, ct));

    /// <summary>Abrir post pelo slug (público). Rota `{slug}` fica depois de `{id:guid}`
    /// nas escritas, mas não conflita: aquelas são PUT/DELETE.</summary>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetalheResponse>> Abrir(string slug, CancellationToken ct) =>
        Ok(await mediator.Send(new AbrirPostCommand(slug), ct));

    [HttpPost]
    public async Task<ActionResult<PostResponse>> Criar(CriarPostRequest req, CancellationToken ct)
    {
        var command = new CriarPostCommand(AutorId, req.Titulo, req.ImagemCapa, req.TagIds, req.Conteudo);

        return Ok(await mediator.Send(command, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostResponse>> Editar(Guid id, EditarPostRequest req, CancellationToken ct)
    {
        var command = new EditarPostCommand(id, AutorId, req.Titulo, req.ImagemCapa, req.TagIds, req.Conteudo);

        return Ok(await mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeletarPostCommand(id, AutorId), ct);

        return NoContent();
    }

    private Guid AutorId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
