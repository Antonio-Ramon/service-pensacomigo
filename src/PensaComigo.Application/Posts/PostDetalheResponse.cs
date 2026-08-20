using PensaComigo.Application.Etapas;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Posts;

/// <summary>Post aberto: o resumo + o corpo (blocos) + autor e tags.</summary>
public record PostDetalheResponse(
    Guid Id,
    string Titulo,
    string? Dek,
    string Slug,
    string ImagemCapa,
    IReadOnlyList<Bloco> Conteudo,
    int TempoLeitura,
    int QtdCurtidas,
    int QtdVisualizacoes,
    DateTime DataCriacao,
    DateTime DataAtualizacao,
    AutorResponse Autor,
    IReadOnlyList<TagResponse> Tags,
    DateTime? DataPublicacao,
    IReadOnlyList<Mood> Moods,
    EtapaResponse? Etapa);

public record AutorResponse(Guid Id, string Nome, string ImagemUrl, string? Bio = null);
