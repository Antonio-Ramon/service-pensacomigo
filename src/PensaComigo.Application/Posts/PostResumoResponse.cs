namespace PensaComigo.Application.Posts;

/// <summary>Card da listagem: só o que a home precisa desenhar. O <c>Conteudo</c> (jsonb inteiro)
/// fica de fora de propósito — 20 posts por página com o corpo junto seria um payload absurdo.</summary>
public record PostResumoResponse(
    Guid Id,
    string Titulo,
    string Slug,
    string ImagemCapa,
    int TempoLeitura,
    int QtdCurtidas,
    int QtdVisualizacoes,
    DateTime DataCriacao);
