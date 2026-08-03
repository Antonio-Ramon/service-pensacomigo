using FluentValidation;

namespace PensaComigo.Application.Imagens.UrlUpload;

/// <summary>
/// Fronteira de confiança: o único pedaço do nome de arquivo que sobrevive é a extensão,
/// e ela é whitelist. Falha → 422 pelo ValidationBehavior (Fatia 5).
/// </summary>
public class GerarUrlUploadQueryValidator : AbstractValidator<GerarUrlUploadQuery>
{
    private static readonly string[] ExtensoesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];

    public GerarUrlUploadQueryValidator()
    {
        RuleFor(q => q.NomeArquivo)
            .NotEmpty().WithMessage("O nome do arquivo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do arquivo tem no máximo 200 caracteres.")
            .Must(nome => ExtensoesPermitidas.Contains(Path.GetExtension(nome).ToLowerInvariant()))
            .WithMessage($"A imagem precisa ser {string.Join(", ", ExtensoesPermitidas)}.");
    }
}
