using FluentValidation;

namespace PensaComigo.Application.Auth.Login;

/// <summary>
/// Primeiro validator real. O ValidationBehavior (Fatia 5) o acha sozinho
/// (AddValidatorsFromAssembly) e roda antes do handler; falha → 422.
/// </summary>
public class LoginGoogleCommandValidator : AbstractValidator<LoginGoogleCommand>
{
    public LoginGoogleCommandValidator()
    {
        RuleFor(c => c.IdToken)
            .NotEmpty().WithMessage("O token do Google é obrigatório.");
    }
}
