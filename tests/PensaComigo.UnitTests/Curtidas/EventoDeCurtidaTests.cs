using PensaComigo.Application.Curtidas;
using PensaComigo.Application.Curtidas.Curtir;
using PensaComigo.Application.Curtidas.Descurtir;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Repositories;
using PensaComigo.UnitTests.Fakes;

namespace PensaComigo.UnitTests.Curtidas;

/// <summary>
/// Trava a regra do evento de curtida: só entra na fila quando o contador REALMENTE mudou.
/// O sinal disso é o <c>null</c> devolvido pelo <c>AjustarCurtidasAsync</c> (nenhuma linha
/// casou) — anunciar assim mesmo empurraria um número velho para todo mundo no grupo.
/// </summary>
public class EventoDeCurtidaTests
{
    private static readonly Guid PostId = Guid.Parse("11111111-1111-1111-1111-111111111111");

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
