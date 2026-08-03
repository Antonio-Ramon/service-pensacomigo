namespace PensaComigo.Application.Posts;

/// <summary>O que a API devolve ao criar/editar um post. Os campos calculados
/// (<c>Slug</c>, <c>TempoLeitura</c>) voltam porque o cliente não sabe derivá-los.</summary>
public record PostResponse(Guid Id, string Titulo, string Slug, int TempoLeitura);
