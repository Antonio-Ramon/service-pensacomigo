using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Auth.Login;

/// <summary>
/// O coração do caso de uso. O Controller só faz Send(command); toda a regra vive aqui.
/// Fluxo: valida token Google → acha usuário do seed por email → recusa fora do seed →
/// preenche google_id/foto no 1º login → emite JWT.
/// </summary>
public class LoginGoogleCommandHandler(
    IGoogleTokenValidator google,
    IUsuarioRepository usuarios,
    IJwtTokenGenerator jwt) : IRequestHandler<LoginGoogleCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginGoogleCommand cmd, CancellationToken ct)
    {
        var conta = await google.ValidarAsync(cmd.IdToken, ct);

        var usuario = await usuarios.ObterPorEmailAsync(conta.Email, ct)
            ?? throw new NaoEncontradoException("Usuário", conta.Email);

        // Primeiro login: puxa google_id e foto da conta Google (UnitOfWorkBehavior commita).
        if (usuario.GoogleId is null)
        {
            usuario.GoogleId = conta.GoogleId;
            if (conta.FotoUrl is not null)
                usuario.ImagemUrl = conta.FotoUrl;
        }

        return new LoginResponse(jwt.Gerar(usuario), usuario.Nome, usuario.ImagemUrl);
    }
}
