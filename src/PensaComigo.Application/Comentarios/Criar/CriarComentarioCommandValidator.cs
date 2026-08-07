using FluentValidation;
using PensaComigo.Application.Common;

namespace PensaComigo.Application.Comentarios.Criar;

public class CriarComentarioCommandValidator : AbstractValidator<CriarComentarioCommand>
{
    public CriarComentarioCommandValidator()
    {
        RuleFor(c => c.Autor)
            .NotEmpty().WithMessage("Diga seu nome para comentar.")
            .MaximumLength(80).WithMessage("O nome tem no máximo 80 caracteres.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("O comentário não pode ser vazio.")
            .MaximumLength(2000).WithMessage("O comentário tem no máximo 2000 caracteres.")
            // Filtro de palavrão é VALIDAÇÃO, não regra do handler: reprovado aqui, o
            // texto sujo nem chega perto do banco (ValidationBehavior barra antes → 422).
            .Must(texto => !FiltroPalavrao.Contem(texto))
            .WithMessage("Revise seu comentário: ele contém termos não permitidos.");

        // Também vale pro nome: ninguém assina "Fulano <palavrão>".
        RuleFor(c => c.Autor)
            .Must(nome => !FiltroPalavrao.Contem(nome))
            .WithMessage("Revise o nome: ele contém termos não permitidos.");
    }
}
