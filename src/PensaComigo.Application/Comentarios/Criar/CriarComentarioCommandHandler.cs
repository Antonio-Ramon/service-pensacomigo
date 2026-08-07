using MediatR;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.Application.Comentarios.Criar;

/// <summary>
/// Moderação automática estilo YouTube: passou no validator (palavrão) e no limitador
/// (spam), publica na hora — <c>Aprovado = true</c>, sem fila de aprovação.
/// </summary>
public class CriarComentarioCommandHandler(
    IPostRepository posts,
    IComentarioRepository comentarios,
    LimitadorDeComentarios limitador)
    : IRequestHandler<CriarComentarioCommand, ComentarioResponse>
{
    public async Task<ComentarioResponse> Handle(CriarComentarioCommand cmd, CancellationToken ct)
    {
        // Primeiro de tudo: quem está inundando não merece nem um SELECT.
        limitador.Registrar(cmd.Visitante);

        if (!await posts.ExistePorIdAsync(cmd.PostId, ct))
            throw new NaoEncontradoException("Post", cmd.PostId);

        if (cmd.ParentId is Guid paiId)
        {
            var pai = await comentarios.ObterPorIdAsync(paiId, ct)
                      ?? throw new NaoEncontradoException("Comentário", paiId);

            // O schema aceita árvore infinita (parent_id é auto-referência). Quem trava em
            // 1 nível é esta linha — decisão de negócio #7, deliberadamente fora do banco.
            if (pai.ParentId is not null)
                throw new RegraDeNegocioException("Só é possível responder a um comentário original.");

            // Sem isto dava pra pendurar uma resposta num comentário de OUTRO post,
            // e ela apareceria na conversa errada.
            if (pai.PostId != cmd.PostId)
                throw new RegraDeNegocioException("O comentário respondido não é deste post.");
        }

        var comentario = new Comentario
        {
            Id = Guid.NewGuid(),
            PostId = cmd.PostId,
            ParentId = cmd.ParentId,
            Autor = cmd.Autor.Trim(),
            Conteudo = cmd.Conteudo.Trim(),
            Aprovado = true,
        };

        await comentarios.AdicionarAsync(comentario, ct);

        return new ComentarioResponse(
            comentario.Id, comentario.PostId, comentario.ParentId,
            comentario.Autor, comentario.Conteudo, comentario.Aprovado);
    }
}
