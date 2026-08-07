using MediatR;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Comentarios.Criar;
using PensaComigo.Web.Visitantes;

namespace PensaComigo.Web.Controllers;

/// <summary>
/// Comentário é do post: a rota é aninhada e o <c>postId</c> sai dela, não do corpo.
/// Sem <c>[Authorize]</c> — leitor comenta sem conta; quem contém abuso é o rate limit.
/// </summary>
[ApiController]
[Route("api/v1/posts/{postId:guid}/comentarios")]
public class ComentariosController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ComentarioResponse>> Criar(
        Guid postId, CriarComentarioRequest req, CancellationToken ct)
    {
        var command = new CriarComentarioCommand(
            postId, req.ParentId, req.Autor, req.Conteudo, HashVisitante.De(HttpContext));

        return Ok(await mediator.Send(command, ct));
    }
}
