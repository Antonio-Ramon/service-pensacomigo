using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PensaComigo.Application.Auth;
using PensaComigo.Domain.Entities;

namespace PensaComigo.Web.Auth;

/// <summary>
/// Impl real do seam da Fatia 9. Emite o JWT PRÓPRIO com a MESMA chave/issuer/audience
/// que o <c>Program.cs</c> valida desde a Fatia 7 — assina aqui, valida lá.
/// </summary>
public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
{
    public string Gerar(Usuario usuario)
    {
        var jwt = config.GetSection("Jwt");
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        // Claims = o que o app confia sobre o portador do token. Lidas via User na Fatia 11.
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("is_admin", usuario.IsAdmin.ToString()),
        ];

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
