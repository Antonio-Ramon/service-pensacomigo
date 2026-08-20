using MediatR;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Obter;

public class ObterPostQueryHandler(IPostRepository posts)
    : IRequestHandler<ObterPostQuery, PostDetalheResponse>
{
    public async Task<PostDetalheResponse> Handle(ObterPostQuery q, CancellationToken ct)
    {
        var post = await posts.ObterDetalhePorIdAsync(q.Id, ct);

        // Não é dono → 404, não 403: "existe, mas não é seu" vaza o acervo alheio.
        if (post is null || post.AutorId != q.AutorId)
            throw new NaoEncontradoException("Post", q.Id.ToString());

        return new PostDetalheResponse(
            post.Id, post.Titulo, post.Dek, post.Slug, post.ImagemCapa,
            post.Conteudo.OrderBy(b => b.Ordem).ToList(),
            post.TempoLeitura, post.QtdCurtidas, post.QtdVisualizacoes,
            post.DataCriacao, post.DataAtualizacao,
            new AutorResponse(post.Autor.Id, post.Autor.Nome, post.Autor.ImagemUrl, post.Autor.Bio),
            post.Tags.Select(t => new TagResponse(t.Id, t.Nome, t.Slug)).ToList(),
            post.DataPublicacao,
            post.Moods,
            post.Etapa is null ? null : Etapas.EtapaResponse.De(post.Etapa));
    }
}
