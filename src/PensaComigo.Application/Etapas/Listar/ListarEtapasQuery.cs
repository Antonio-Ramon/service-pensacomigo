using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Etapas.Listar;

/// <summary>Catálogo da trilha (4 etapas): sem filtro nem paginação de propósito.</summary>
public record ListarEtapasQuery : IQuery<IReadOnlyList<EtapaResponse>>;
