using MediatR;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Auth.Login;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{
    /// <summary>Controller magro: recebe, delega ao MediatR, devolve. Zero regra aqui.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginGoogleCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));
}
