namespace PensaComigo.Application.Comentarios.Criar;

/// <summary>Corpo do POST. Nem <c>PostId</c> (vem da rota) nem <c>Visitante</c>
/// (vem do servidor) estão aqui — igual ao <c>AutorId</c> na criação de post.</summary>
/// <remarks><c>Autor</c> é opcional só para quem está logado: nesse caso o nome sai da
/// conta, e mandar um aqui não muda nada.</remarks>
public record CriarComentarioRequest(Guid? ParentId, string? Autor, string Conteudo);
