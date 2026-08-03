using FluentValidation;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts.Criar;

/// <summary>
/// Fronteira de confiança do post. O <c>Bloco</c> é flat (todos os campos coexistem),
/// então é aqui que se cobra a coerência: cada tipo exige o seu campo preenchido.
/// </summary>
public class CriarPostCommandValidator : AbstractValidator<CriarPostCommand>
{
    public CriarPostCommandValidator()
    {
        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(200).WithMessage("O título tem no máximo 200 caracteres.");

        RuleFor(c => c.ImagemCapa)
            .NotEmpty().WithMessage("A imagem de capa é obrigatória.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("O post precisa de pelo menos um bloco de conteúdo.");

        // RuleForEach: a mesma regra em cada item da lista, com índice no erro (Conteudo[2]).
        RuleForEach(c => c.Conteudo)
            .Must(Coerente)
            .WithMessage("Bloco incompleto: cada tipo exige o seu campo (texto→html, imagem→path, link→url).");
    }

    private static bool Coerente(Bloco b) => b.Tipo switch
    {
        TipoBloco.Texto => !string.IsNullOrWhiteSpace(b.Html),
        TipoBloco.Imagem => !string.IsNullOrWhiteSpace(b.ImagemPath),
        TipoBloco.Link => !string.IsNullOrWhiteSpace(b.LinkUrl),
        _ => false,
    };
}
