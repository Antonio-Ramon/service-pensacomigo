using FluentValidation;

namespace PensaComigo.Application.Imagens.Enviar;

/// <summary>
/// Fronteira de confiança: do nome do arquivo só a extensão sobrevive, e ela é whitelist.
/// Falha → 422 pelo ValidationBehavior (Fatia 5), antes de um único byte subir.
/// </summary>
public class EnviarImagemCommandValidator : AbstractValidator<EnviarImagemCommand>
{
    public EnviarImagemCommandValidator()
    {
        RuleFor(c => c.NomeArquivo)
            .NotEmpty().WithMessage("O arquivo é obrigatório.")
            .Must(nome => ImagensPermitidas.Tipos.ContainsKey(Path.GetExtension(nome).ToLowerInvariant()))
            .WithMessage($"A imagem precisa ser {string.Join(", ", ImagensPermitidas.Tipos.Keys)}.");

        RuleFor(c => c.Tamanho)
            .GreaterThan(0).WithMessage("O arquivo está vazio.")
            .LessThanOrEqualTo(ImagensPermitidas.TamanhoMaximoBytes)
            .WithMessage($"A imagem tem no máximo {ImagensPermitidas.TamanhoMaximoBytes / 1024 / 1024} MB.");
    }
}
