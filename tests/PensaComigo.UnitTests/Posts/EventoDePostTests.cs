using PensaComigo.Application.Messaging;
using PensaComigo.Application.Posts;
using PensaComigo.Application.Posts.Deletar;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Exceptions;
using PensaComigo.UnitTests.Fakes;

namespace PensaComigo.UnitTests.Posts;

/// <summary>
/// Trava o anúncio do post que sai do ar. É invisível no código: apagar a linha do
/// <c>eventos.Adicionar</c> compila, deleta o post e passa em todo teste de rota.
/// </summary>
public class EventoDePostTests
{
    private static readonly Guid Autor = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Post Existente() => new()
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Titulo = "Meditar",
        Slug = "meditar",
        ImagemCapa = "capa.jpg",
        AutorId = Autor,
    };

    [Fact]
    public async Task Deletar_enfileira_post_removido_com_o_slug()
    {
        var post = Existente();
        var fila = new FilaDeEventos();
        var repo = new PostRepositorioFake(post: post);

        await new DeletarPostCommandHandler(repo, fila).Handle(new DeletarPostCommand(post.Id, Autor), default);

        Assert.Same(post, repo.Removido);
        var evento = Assert.IsType<PostRemovido>(Assert.Single(fila.Drenar()));
        Assert.Equal(post.Id, evento.PostId);
        Assert.Equal("meditar", evento.Slug);
    }

    /// <summary>Não é seu → 404 antes de qualquer coisa. Anunciar aqui vazaria que o post existe.</summary>
    [Fact]
    public async Task Deletar_post_de_outro_autor_nao_anuncia_nada()
    {
        var fila = new FilaDeEventos();
        var repo = new PostRepositorioFake(post: Existente());
        var handler = new DeletarPostCommandHandler(repo, fila);

        await Assert.ThrowsAsync<NaoEncontradoException>(
            () => handler.Handle(new DeletarPostCommand(Existente().Id, Guid.NewGuid()), default));

        Assert.Empty(fila.Drenar());
    }
}
