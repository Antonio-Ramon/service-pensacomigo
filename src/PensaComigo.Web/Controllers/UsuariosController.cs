using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensaComigo.Application.Usuarios.Perfil;

namespace PensaComigo.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsuariosController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Perfil do próprio usuário logado. [Authorize] exige um JWT válido; sem ele → 401 automático.
    /// O `User` (ClaimsPrincipal) é preenchido pelo middleware de autenticação a partir do token.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<PerfilResponse>> EuMesmo(CancellationToken ct)
    {
        // A claim `sub` guarda o id (posto lá no JwtTokenGenerator). MapInboundClaims=false → nome intacto.
        var id = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        return Ok(await mediator.Send(new ObterPerfilQuery(id), ct));
    }
}
