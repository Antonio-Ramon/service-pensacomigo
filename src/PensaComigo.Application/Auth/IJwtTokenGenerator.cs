using PensaComigo.Domain.Entities;

namespace PensaComigo.Application.Auth;

/// <summary>
/// Seam: emite o JWT PRÓPRIO do app (claims: id, email, is_admin).
/// Impl real (chave simétrica de Jwt:Key) chega na Fatia 10.
/// </summary>
public interface IJwtTokenGenerator
{
    string Gerar(Usuario usuario);
}
