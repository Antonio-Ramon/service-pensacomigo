namespace PensaComigo.Domain.Entities;

/// <summary>Etapa da trilha de leitura ("da pergunta ao descanso").
/// Catálogo pequeno e estável: nasce por seed, sem CRUD.</summary>
public class Etapa
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descricao { get; set; } = null!;

    /// <summary>Referências bíblicas da etapa (texto livre, ex.: "Sl 13; Hc 1").</summary>
    public string? Refs { get; set; }
}
