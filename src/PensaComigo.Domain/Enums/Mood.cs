using System.Text.Json.Serialization;

namespace PensaComigo.Domain.Enums;

/// <summary>Estado de chegada do leitor — filtro editorial "Como você chega hoje?".</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Mood
{
    Cansado = 0,
    EmDuvida = 1,
    ComMedo = 2,
    Grato = 3,
    EmLuto = 4,
}
