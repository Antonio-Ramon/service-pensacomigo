using FluentValidation;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts;

/// <summary>
/// Fronteira de confiança do post, comum a criar e editar. O <c>Bloco</c> é flat
/// (todos os campos coexistem), então é aqui que se cobra a coerência: cada tipo
/// exige o seu campo preenchido.
/// </summary>
/// <remarks>Genérica na constraint <see cref="IPostEscrita"/>: as regras valem para
/// qualquer command com esse formato. Cada command herda a sua versão fechada, e é
/// a classe fechada que o <c>AddValidatorsFromAssembly</c> acha (genérica aberta ele ignora).</remarks>
public abstract class PostEscritaValidator<T> : AbstractValidator<T> where T : IPostEscrita
{
    protected PostEscritaValidator()
    {
        RuleFor(c => c.Titulo)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(200).WithMessage("O título tem no máximo 200 caracteres.");

        RuleFor(c => c.ImagemCapa)
            .NotEmpty().WithMessage("A imagem de capa é obrigatória.");

        RuleFor(c => c.Conteudo)
            .NotEmpty().WithMessage("O post precisa de pelo menos um bloco de conteúdo.");

        // Lista vazia é legítima (post sem tag); ausente não é — sem o MVC barrando o null,
        // quem não checar aqui recebe NullReferenceException lá no handler.
        RuleFor(c => c.TagIds)
            .NotNull().WithMessage("Informe as tags do post (lista vazia se não houver).");

        RuleFor(c => c.Dek)
            .MaximumLength(200).WithMessage("O dek tem no máximo 200 caracteres.");

        RuleFor(c => c.Moods)
            .NotNull().WithMessage("Informe os moods do post (lista vazia se não houver).");

        RuleForEach(c => c.Moods)
            .IsInEnum().WithMessage("Mood inválido.");

        RuleFor(c => c.Status)
            .IsInEnum().WithMessage("Status inválido: 0 (Rascunho), 1 (Publicado) ou 2 (Agendado).");

        // Agendar sem data (ou com data passada) é um post que nunca entra no ar.
        RuleFor(c => c.DataPublicacao)
            .NotNull().WithMessage("Post agendado exige DataPublicacao.")
            .GreaterThan(_ => DateTime.UtcNow).WithMessage("DataPublicacao do agendamento precisa ser futura.")
            .When(c => c.Status == StatusPost.Agendado);

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
