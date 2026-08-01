using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Auth.Login;

/// <summary>
/// Login: o frontend faz o OAuth e manda o <paramref name="IdToken"/> do Google.
/// É um Command (não Query) porque o primeiro login grava google_id/foto → commita.
/// </summary>
public record LoginGoogleCommand(string IdToken) : ICommand<LoginResponse>;
