using PensaComigo.Domain.Enums;

namespace PensaComigo.Domain.ValueObjects;

/// <summary>
/// Bloco de conteúdo de um post. Modelo "flat": todos os campos possíveis
/// coexistem, e o Tipo indica quais estão preenchidos.
/// Serializado como parte do array jsonb da coluna Conteudo do Post.
/// </summary>
public class Bloco
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TipoBloco Tipo {  get; set; }
    public int Ordem { get; set; }

    // --- Tipo: Texto ---
    public string? Html { get; set; }

    // --- Tipo: Imagem ---
    public string? ImagemPath { get; set; }
    public string? ImagemUrl { get; set; }
    public string? Alt { get; set; }
    public double? AspectRatio { get; set; }

    // --- Tipo: Link ---
    public string? LinkUrl { get; set; }
    public string? LinkTitulo { get; set; }
    public string? LinkDescricao { get; set; }
    public string? LinkThumbnail { get; set; }
    public string? LinkSiteName { get; set; }
}