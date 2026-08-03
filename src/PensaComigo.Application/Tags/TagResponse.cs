namespace PensaComigo.Application.Tags;

/// <summary>O que a API devolve pra uma tag. Mesmo shape no criar e no listar.</summary>
public record TagResponse(Guid Id, string Nome, string Slug);
