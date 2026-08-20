using PensaComigo.Application.Tags;
using PensaComigo.Domain.Enums;

namespace PensaComigo.Application.Posts;

/// <summary>Card da listagem: só o que a home precisa desenhar (tags em pílula + autor incluídos).
/// O <c>Conteudo</c> (jsonb inteiro) fica de fora de propósito — 20 posts por página com o corpo
/// junto seria um payload absurdo.</summary>
public record PostResumoResponse(
    Guid Id,
    string Titulo,
    string Slug,
    string ImagemCapa,
    int TempoLeitura,
    int QtdCurtidas,
    int QtdVisualizacoes,
    DateTime DataCriacao,
    AutorResponse Autor,
    IReadOnlyList<TagResponse> Tags,
    StatusPost Status,
    DateTime? DataPublicacao);   // null enquanto rascunho
