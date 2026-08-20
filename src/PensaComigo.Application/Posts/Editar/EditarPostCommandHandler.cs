using MediatR;
using PensaComigo.Application.Common;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Editar;

/// <summary>
/// Editar não constrói entidade nova: carrega a RASTREADA, muda propriedade e pronto —
/// o change tracker (Fatia 16) monta o UPDATE e o UnitOfWorkBehavior commita.
/// Slug não entra: congelou na criação.
/// </summary>
public class EditarPostCommandHandler(IPostRepository posts, ITagRepository tags, IEtapaRepository etapas)
    : IRequestHandler<EditarPostCommand, PostResponse>
{
    public async Task<PostResponse> Handle(EditarPostCommand cmd, CancellationToken ct)
    {
        var post = await posts.ObterParaEdicaoAsync(cmd.Id, ct);

        // Não é dono → 404, não 403: responder "existe, mas não é seu" já vaza o acervo alheio.
        if (post is null || post.AutorId != cmd.AutorId)
            throw new NaoEncontradoException("Post", cmd.Id.ToString());

        var vinculadas = await tags.ObterPorIdsAsync(cmd.TagIds, ct);
        var faltando = cmd.TagIds.Except(vinculadas.Select(t => t.Id)).ToList();
        if (faltando.Count > 0)
            throw new NaoEncontradoException("Tag", string.Join(", ", faltando));

        if (cmd.EtapaId is Guid etapaId && !await etapas.ExistePorIdAsync(etapaId, ct))
            throw new NaoEncontradoException("Etapa", etapaId);

        // Fronteira de confiança: HTML dos blocos Texto passa pela whitelist antes de persistir.
        SanitizadorHtml.SanitizarBlocos(cmd.Conteudo);

        post.Titulo = cmd.Titulo.Trim();
        post.Dek = string.IsNullOrWhiteSpace(cmd.Dek) ? null : cmd.Dek.Trim();
        post.ImagemCapa = cmd.ImagemCapa;
        post.Conteudo = [.. cmd.Conteudo.OrderBy(b => b.Ordem)];
        post.TempoLeitura = CalculadoraTempoLeitura.Calcular(cmd.Conteudo);
        post.Moods = [.. cmd.Moods.Distinct()];
        post.EtapaId = cmd.EtapaId;
        post.DataAtualizacao = DateTime.UtcNow;

        post.Status = cmd.Status;
        // DataPublicacao congela na PRIMEIRA publicação: republicar não reposiciona o post no feed.
        // Publicar um agendado ainda futuro carimba agora (a data agendada nunca chegou a valer).
        if (cmd.Status == StatusPost.Publicado && (post.DataPublicacao is null || post.DataPublicacao > DateTime.UtcNow))
            post.DataPublicacao = DateTime.UtcNow;
        else if (cmd.Status == StatusPost.Agendado)
            post.DataPublicacao = cmd.DataPublicacao;

        // Trocar a coleção inteira: o EF compara com o que carregou e emite só o
        // delta em post_tags (DELETE das que saíram, INSERT das que entraram).
        post.Tags = vinculadas;

        return new PostResponse(post.Id, post.Titulo, post.Slug, post.TempoLeitura);
    }
}
