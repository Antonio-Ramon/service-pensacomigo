using FluentValidation;

namespace PensaComigo.Application.Links.Preview;

public class ObterPreviewLinkQueryValidator : AbstractValidator<ObterPreviewLinkQuery>
{
    public ObterPreviewLinkQueryValidator()
    {
        RuleFor(q => q.Url)
            .NotEmpty().WithMessage("Informe a url do link.")
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out var uri)
                       && uri.Scheme is "http" or "https")
            .WithMessage("A url precisa ser http ou https absoluta.");
    }
}
