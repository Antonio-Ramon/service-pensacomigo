using PensaComigo.Domain.Entities;

namespace PensaComigo.Application.Etapas;

public record EtapaResponse(Guid Id, int Numero, string Titulo, string Descricao, string? Refs)
{
    public static EtapaResponse De(Etapa e) => new(e.Id, e.Numero, e.Titulo, e.Descricao, e.Refs);
}
