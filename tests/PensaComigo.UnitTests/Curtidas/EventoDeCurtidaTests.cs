using Gridify;
using PensaComigo.Application.Curtidas;
using PensaComigo.Application.Curtidas.Curtir;
using PensaComigo.Application.Curtidas.Descurtir;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.UnitTests.Curtidas;

/// <summary>
/// Trava a regra do evento de curtida: só entra na fila quando o contador REALMENTE mudou.
/// O sinal disso é o <c>null</c> devolvido pelo <c>AjustarCurtidasAsync</c> (nenhuma linha
/// casou) — anunciar assim mesmo empurraria um número velho para todo mundo no grupo.
/// </summary>
public class EventoDeCurtidaTests
{
    private static readonly Guid PostId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary><paramref name="contador"/> é o que o UPDATE ... RETURNING devolveria.</summary>
    private sealed class PostRepositorioFake(int? contador) : IPostRepository
    {
        public Task<int?> AjustarCurtidasAsync(Guid id, int delta, CancellationToken ct = default) =>
            Task.FromResult(contador);

        public Task<bool> ExistePorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(true);

        // O resto da interface não participa deste caso de uso.
        public Task<Post?> ObterPorIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Guid?> ObterAutorIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Pagina<Post>> ListarAsync(IGridifyQuery consulta, bool incluirRascunhos = false, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Post?> ObterPorSlugAsync(string slug, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Post?> ObterDetalhePorIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task IncrementarVisualizacoesAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> ListarSlugsComPrefixoAsync(string prefixo, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Post?> ObterParaEdicaoAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task AdicionarAsync(Post post, CancellationToken ct = default) => throw new NotSupportedException();
        public void Remover(Post post) => throw new NotSupportedException();
    }

    private sealed class LikeRepositorioFake(Like? existente) : ILikeRepository
    {
        public Task<bool> ExisteAsync(Guid postId, string viewerHash, CancellationToken ct = default) =>
            Task.FromResult(existente is not null);

        public Task<Like?> ObterAsync(Guid postId, string viewerHash, CancellationToken ct = default) =>
            Task.FromResult(existente);

        public Task AdicionarAsync(Like like, CancellationToken ct = default) => Task.CompletedTask;

        public void Remover(Like like) { }
    }

    private static Like Curtida() => new() { Id = Guid.NewGuid(), PostId = PostId, ViewerHash = "visitante" };

    [Fact]
    public async Task Curtida_nova_enfileira_o_contador_devolvido_pelo_banco()
    {
        var fila = new FilaDeEventos();
        var handler = new CurtirPostCommandHandler(new PostRepositorioFake(7), new LikeRepositorioFake(null), fila);

        await handler.Handle(new CurtirPostCommand(PostId, "visitante"), default);

        var evento = Assert.IsType<CurtidasAtualizadas>(Assert.Single(fila.Drenar()));
        Assert.Equal(PostId, evento.PostId);
        Assert.Equal(7, evento.Qtd);
    }

    [Fact]
    public async Task Curtir_duas_vezes_nao_anuncia_nada_na_segunda()
    {
        var fila = new FilaDeEventos();
        var handler = new CurtirPostCommandHandler(new PostRepositorioFake(7), new LikeRepositorioFake(Curtida()), fila);

        await handler.Handle(new CurtirPostCommand(PostId, "visitante"), default);

        Assert.Empty(fila.Drenar());
    }

    [Fact]
    public async Task Descurtir_enfileira_o_contador_novo()
    {
        var fila = new FilaDeEventos();
        var handler = new DescurtirPostCommandHandler(new PostRepositorioFake(6), new LikeRepositorioFake(Curtida()), fila);

        await handler.Handle(new DescurtirPostCommand(PostId, "visitante"), default);

        Assert.Equal(6, Assert.IsType<CurtidasAtualizadas>(Assert.Single(fila.Drenar())).Qtd);
    }

    /// <summary>A guarda `qtd_curtidas + delta >= 0` do UPDATE não casou nenhuma linha:
    /// o contador não mudou, então não há nada para anunciar.</summary>
    [Fact]
    public async Task Update_que_nao_casa_linha_nenhuma_nao_anuncia()
    {
        var fila = new FilaDeEventos();
        var handler = new DescurtirPostCommandHandler(new PostRepositorioFake(null), new LikeRepositorioFake(Curtida()), fila);

        await handler.Handle(new DescurtirPostCommand(PostId, "visitante"), default);

        Assert.Empty(fila.Drenar());
    }
}
