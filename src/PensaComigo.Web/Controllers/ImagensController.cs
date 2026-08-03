using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Imagens.UrlUpload;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImagensController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Autor pede permissão de upload; o binário sobe direto no Supabase com a URL devolvida
    /// (Decisão #14 — não passa por aqui). O dono da pasta sai da claim, não do corpo.
    /// </summary>
    [Authorize]
    [HttpPost("url-upload")]
    public async Task<ActionResult<UrlUploadResponse>> UrlUpload(UrlUploadRequest req, CancellationToken ct)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        return Ok(await mediator.Send(new GerarUrlUploadQuery(usuarioId, req.NomeArquivo), ct));
    }
}
