using MediatR;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Usuarios.Perfil;

/// <summary>
/// Carrega o perfil pelo id da claim. O JWT já provou QUEM é; aqui buscamos os dados
/// frescos (nome/foto) que não cabem no token. Query = sem commit (UnitOfWorkBehavior não escreve).
/// </summary>
public class ObterPerfilQueryHandler(IUsuarioRepository usuarios)
    : IRequestHandler<ObterPerfilQuery, PerfilResponse>
{
    public async Task<PerfilResponse> Handle(ObterPerfilQuery q, CancellationToken ct)
    {
        var u = await usuarios.ObterPorIdAsync(q.UsuarioId, ct)
            ?? throw new NaoEncontradoException("Usuário", q.UsuarioId.ToString());

        return new PerfilResponse(u.Id, u.Nome, u.Email, u.ImagemUrl, u.IsAdmin);
    }
}
