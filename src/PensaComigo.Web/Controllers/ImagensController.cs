using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Imagens;
using PensaComigo.Application.Imagens.Enviar;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImagensController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Autor sobe a imagem por aqui (multipart) e recebe o <c>path</c> pra guardar no post
    /// mais a URL pública pra exibir. O dono da pasta sai da claim, não do corpo.
    /// </summary>
    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ImagensPermitidas.TamanhoMaximoBytes)]
    public async Task<ActionResult<ImagemResponse>> Enviar(IFormFile arquivo, CancellationToken ct)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        // O IFormFile (tipo do ASP.NET) para aqui: a Application recebe só nome, tamanho e Stream.
        await using var conteudo = arquivo.OpenReadStream();
        var command = new EnviarImagemCommand(usuarioId, arquivo.FileName, arquivo.Length, conteudo);

        return Ok(await mediator.Send(command, ct));
    }
}
