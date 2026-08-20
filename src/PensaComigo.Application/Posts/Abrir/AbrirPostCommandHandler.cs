using MediatR;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Abrir;

public class AbrirPostCommandHandler(IPostRepository posts)
    : IRequestHandler<AbrirPostCommand, PostDetalheResponse>
{
    public async Task<PostDetalheResponse> Handle(AbrirPostCommand cmd, CancellationToken ct)
    {
        var post = await posts.ObterPorSlugAsync(cmd.Slug, ct);

        // Rascunho não existe para o público: 404, mesma semântica de "não é seu".
        if (post is null || post.Status != Domain.Enums.StatusPost.Publicado)
            throw new NaoEncontradoException("Post", cmd.Slug);

        // O UPDATE é atômico no banco (ver repo), então o valor que devolvemos é o de antes + 1.
        // ponytail: incremento cru, sem dedup por visitante — se virar métrica séria, deduplicar
        // por viewer_hash como nos likes.
        await posts.IncrementarVisualizacoesAsync(post.Id, ct);

        return new PostDetalheResponse(
            post.Id, post.Titulo, post.Slug, post.ImagemCapa,
            post.Conteudo.OrderBy(b => b.Ordem).ToList(),
            post.TempoLeitura, post.QtdCurtidas, post.QtdVisualizacoes + 1,
            post.DataCriacao, post.DataAtualizacao,
            new AutorResponse(post.Autor.Id, post.Autor.Nome, post.Autor.ImagemUrl, post.Autor.Bio),
            post.Tags.Select(t => new TagResponse(t.Id, t.Nome, t.Slug)).ToList(),
            post.DataPublicacao);
    }
}
