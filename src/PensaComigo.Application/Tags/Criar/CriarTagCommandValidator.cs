using FluentValidation;

namespace PensaComigo.Application.Tags.Criar;

/// <summary>Achado pelo ValidationBehavior (Fatia 5); roda antes do handler, falha → 422.</summary>
public class CriarTagCommandValidator : AbstractValidator<CriarTagCommand>
{
    public CriarTagCommandValidator()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("O nome da tag é obrigatório.")
            .MaximumLength(50).WithMessage("O nome da tag tem no máximo 50 caracteres.");
    }
}
