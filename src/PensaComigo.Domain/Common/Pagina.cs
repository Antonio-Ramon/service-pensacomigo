namespace PensaComigo.Domain.Common;

/// <summary>
/// Envelope padrão de toda listagem da API: <c>{ items, totalItems }</c>.
/// TotalItems é a contagem ANTES de paginar (o cliente precisa dela pra montar o paginador).
/// Vive no Domain só porque é o único projeto que todos enxergam (repo devolve, Application projeta).
/// </summary>
public record Pagina<T>(IReadOnlyList<T> Items, int TotalItems);
