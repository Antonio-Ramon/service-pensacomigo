using MediatR;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>Sem regra: pede a página de raízes ao repo e projeta pra árvore rasa.
/// A única conta é marcar quem comentou sendo o dono do post — daí o id do autor.</summary>
public class ListarComentariosQueryHandler(IComentarioRepository comentarios, IPostRepository posts)
    : IRequestHandler<ListarComentariosQuery, Pagina<ComentarioListaResponse>>
{
    public async Task<Pagina<ComentarioListaResponse>> Handle(ListarComentariosQuery q, CancellationToken ct)
    {
        var pagina = await comentarios.ListarAsync(q.PostId, q.Consulta, q.IncluirOcultos, ct);
        // Só o id do dono, não o post inteiro: carregar a entidade traria o `conteudo` jsonb junto.
        var autorDoPost = await posts.ObterAutorIdAsync(q.PostId, ct);

        return new Pagina<ComentarioListaResponse>(
            pagina.Items.Select(c => new ComentarioListaResponse(
                c.Id, c.Autor, c.Conteudo, c.DataCriacao, c.Aprovado,
                c.Usuario?.ImagemUrl, EhAutor(c.UsuarioId, autorDoPost),
                c.Respostas.Select(r => new RespostaResponse(
                    r.Id, r.Autor, r.Conteudo, r.DataCriacao, r.Aprovado,
                    r.Usuario?.ImagemUrl, EhAutor(r.UsuarioId, autorDoPost))).ToList()))
                .ToList(),
            pagina.TotalItems);
    }

    // Anônimo (usuário null) nunca é o autor, mesmo que o post não tenha dono conhecido.
    private static bool EhAutor(Guid? usuarioId, Guid? autorDoPost) =>
        usuarioId is not null && usuarioId == autorDoPost;
}
