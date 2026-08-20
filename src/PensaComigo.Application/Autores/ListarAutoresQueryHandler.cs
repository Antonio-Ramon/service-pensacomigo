using MediatR;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Autores;

public class ListarAutoresQueryHandler(IUsuarioRepository usuarios)
    : IRequestHandler<ListarAutoresQuery, Pagina<AutorResponse>>
{
    public async Task<Pagina<AutorResponse>> Handle(ListarAutoresQuery q, CancellationToken ct)
    {
        var autores = await usuarios.ListarAsync(ct);

        return new Pagina<AutorResponse>(
            autores.Select(u => new AutorResponse(u.Id, u.Nome, u.ImagemUrl, u.Bio)).ToList(),
            autores.Count);
    }
}
