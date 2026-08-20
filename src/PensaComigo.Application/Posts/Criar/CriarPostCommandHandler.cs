using MediatR;
using PensaComigo.Application.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Posts.Criar;

/// <summary>
/// Impureim sandwich: lê (slugs ocupados + tags), decide com funções puras
/// (slug e tempo de leitura), grava. Quem commita é o UnitOfWorkBehavior.
/// </summary>
public class CriarPostCommandHandler(IPostRepository posts, ITagRepository tags, IEtapaRepository etapas)
    : IRequestHandler<CriarPostCommand, PostResponse>
{
    public async Task<PostResponse> Handle(CriarPostCommand cmd, CancellationToken ct)
    {
        // Um round-trip só: traz "meditar", "meditar-2"... e a decisão é pura (Fatia 15).
        var slugBase = GeradorSlug.Gerar(cmd.Titulo);
        var slug = GeradorSlug.ResolverColisao(slugBase, await posts.ListarSlugsComPrefixoAsync(slugBase, ct));

        // Tags PRECISAM ser as entidades rastreadas do banco. Passar `new Tag { Id = ... }`
        // faria o EF tentar INSERIR a tag de novo; aqui ele só insere a linha de post_tags.
        var vinculadas = await tags.ObterPorIdsAsync(cmd.TagIds, ct);
        var faltando = cmd.TagIds.Except(vinculadas.Select(t => t.Id)).ToList();
        if (faltando.Count > 0)
            throw new NaoEncontradoException("Tag", string.Join(", ", faltando));

        if (cmd.EtapaId is Guid etapaId && !await etapas.ExistePorIdAsync(etapaId, ct))
            throw new NaoEncontradoException("Etapa", etapaId);

        // Fronteira de confiança: HTML dos blocos Texto passa pela whitelist antes de persistir.
        SanitizadorHtml.SanitizarBlocos(cmd.Conteudo);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Titulo = cmd.Titulo.Trim(),
            Dek = string.IsNullOrWhiteSpace(cmd.Dek) ? null : cmd.Dek.Trim(),
            Slug = slug,
            ImagemCapa = cmd.ImagemCapa,
            Conteudo = [.. cmd.Conteudo.OrderBy(b => b.Ordem)],
            TempoLeitura = CalculadoraTempoLeitura.Calcular(cmd.Conteudo),
            AutorId = cmd.AutorId,
            Tags = vinculadas,
            Moods = [.. cmd.Moods.Distinct()],
            EtapaId = cmd.EtapaId,
            Status = cmd.Status,
            // Publicar direto na criação já carimba a data que o feed ordena;
            // agendado usa a data escolhida (o validator garante que é futura).
            DataPublicacao = cmd.Status switch
            {
                StatusPost.Publicado => DateTime.UtcNow,
                StatusPost.Agendado => cmd.DataPublicacao,
                _ => null,
            },
        };

        await posts.AdicionarAsync(post, ct);

        return new PostResponse(post.Id, post.Titulo, post.Slug, post.TempoLeitura);
    }
}
