using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gridify;
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
public class ComentariosController(ISender mediator, HashVisitante hash) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ComentarioResponse>> Criar(
        Guid postId, CriarComentarioRequest req, CancellationToken ct)
    {
        // Logado comenta como ELE MESMO: o id sai da claim e o nome é resolvido no handler.
        // Anônimo segue mandando o nome no corpo, que é o caso normal da conversa.
        var command = new CriarComentarioCommand(
            postId, req.ParentId, req.Autor ?? string.Empty, req.Conteudo, hash.De(HttpContext), UsuarioLogado);

        return Ok(await mediator.Send(command, ct));
    }

    /// <summary>Conversa do post (pública). Só aprovados, raízes paginadas com suas respostas.</summary>
    [HttpGet]
    public async Task<ActionResult<Pagina<ComentarioListaResponse>>> Listar(
        Guid postId, [FromQuery] GridifyQuery consulta, CancellationToken ct) =>
        // Página/ordem/filtro vêm da querystring; o post sai da ROTA — o cliente não
        // escolhe de qual post lê, e por isso `postId` não é campo da consulta.
        // Ocultos entram só para admin, e o flag vem da CLAIM, nunca da querystring:
        // é o que faz a tela de moderação enxergar o que escondeu (e poder reexibir).
        Ok(await mediator.Send(
            new ListarComentariosQuery(postId, consulta) { IncluirOcultos = EhAdmin }, ct));

    private bool EhAdmin => User.HasClaim("is_admin", "true");

    private Guid? UsuarioLogado =>
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    /// <summary>Esconde o comentário (aprovado = false). Só admin — ver policy no Program.cs.</summary>
    [Authorize(Policy = "Admin")]
    [HttpPatch("{id:guid}/ocultar")]
    public async Task<IActionResult> Ocultar(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ModerarComentarioCommand(id, Aprovado: false), ct);

        return NoContent();
    }

    /// <summary>Desfaz o ocultar. Só admin — que é quem enxerga o comentário oculto na listagem.</summary>
    [Authorize(Policy = "Admin")]
    [HttpPatch("{id:guid}/reexibir")]
    public async Task<IActionResult> Reexibir(Guid id, CancellationToken ct)
    {
        await mediator.Send(new ModerarComentarioCommand(id, Aprovado: true), ct);

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
