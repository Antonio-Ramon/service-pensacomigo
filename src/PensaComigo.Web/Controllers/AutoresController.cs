using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Autores;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;

namespace PensaComigo.Web.Controllers;

/// <summary>"Quem escreve" da home: público, anônimo (issue #22).</summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AutoresController(ISender mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<Pagina<AutorResponse>>> Listar(CancellationToken ct) =>
        Ok(await mediator.Send(new ListarAutoresQuery(), ct));
}
