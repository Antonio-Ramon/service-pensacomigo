using MediatR;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Etapas.Listar;

public class ListarEtapasQueryHandler(IEtapaRepository etapas)
    : IRequestHandler<ListarEtapasQuery, IReadOnlyList<EtapaResponse>>
{
    public async Task<IReadOnlyList<EtapaResponse>> Handle(ListarEtapasQuery _, CancellationToken ct) =>
        [.. (await etapas.ListarAsync(ct)).Select(EtapaResponse.De)];
}
