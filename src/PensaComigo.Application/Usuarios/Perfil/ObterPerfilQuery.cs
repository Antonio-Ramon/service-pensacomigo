using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Usuarios.Perfil;

/// <summary>
/// Primeiro Query do projeto (lado leitura do CQRS): só lê, não muda nada.
/// O <paramref name="UsuarioId"/> vem da claim `sub` do JWT — o controller extrai, o handler confia.
/// </summary>
public record ObterPerfilQuery(Guid UsuarioId) : IQuery<PerfilResponse>;
