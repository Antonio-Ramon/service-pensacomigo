using MediatR;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Etapas;
using PensaComigo.Application.Etapas.Listar;

namespace PensaComigo.Web.Controllers;

/// <summary>Catálogo público da trilha de leitura (issue #28). Só leitura: as etapas
/// nascem por seed, não há CRUD.</summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EtapasController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EtapaResponse>>> Listar(CancellationToken ct) =>
        Ok(await mediator.Send(new ListarEtapasQuery(), ct));
}
