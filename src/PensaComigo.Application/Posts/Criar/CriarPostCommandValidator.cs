namespace PensaComigo.Application.Posts.Criar;

/// <summary>Só fecha o genérico — as regras vivem no <see cref="PostEscritaValidator{T}"/>.</summary>
public class CriarPostCommandValidator : PostEscritaValidator<CriarPostCommand>;
