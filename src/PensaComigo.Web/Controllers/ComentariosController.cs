using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Comentarios.Criar;
using PensaComigo.Application.Comentarios.Listar;
using PensaComigo.Application.Comentarios.Moderar;
using PensaComigo.Domain.Common;
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

    /// <summary>Conversa do post (pública). Só aprovados, raízes paginadas com suas respostas.</summary>
    [HttpGet]
    public async Task<ActionResult<Pagina<ComentarioListaResponse>>> Listar(
        Guid postId, [FromQuery] ListarComentariosQuery query, CancellationToken ct)
    {
        // A rota manda: o binder já preencheu o resto da querystring, aqui o postId
        // é sobrescrito com o valor da URL — o cliente não escolhe de qual post lê.
        query.PostId = postId;

        return Ok(await mediator.Send(query, ct));
    }

    /// <summary>Esconde o comentário (aprovado = false). Só admin — ver policy no Program.cs.</summary>
    [Authorize(Policy = "Admin")]
    [HttpPatch("{id:guid}/ocultar")]
    public async Task<IActionResult> Ocultar(Guid id, CancellationToken ct)
    {
        await mediator.Send(new OcultarComentarioCommand(id), ct);

        return NoContent();
    }

    [Authorize(Policy = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deletar(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeletarComentarioCommand(id), ct);

        return NoContent();
    }
}
