using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Enums;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Ticket 08: curtir/descurtir anônimo, deduplicado por visitante.
/// <para>
/// Cada teste usa um <b>User-Agent próprio</b>: o hash do visitante nasce de IP + User-Agent,
/// e é ele que o índice único usa — com um UA só, um teste curtiria "de novo" pelo outro.
/// </para>
/// </summary>
public class CurtidasTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Curtir_duas_vezes_conta_uma_so()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        var primeira = await leitor.PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);
        var segunda = await leitor.PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);

        // Idempotente: a segunda não é erro, é o mesmo 204.
        Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, segunda.StatusCode);

        Assert.Equal(1, await ContarLikesAsync(post.Id));
        Assert.Equal(1, await ContadorAsync(post.Id));
    }

    [Fact]
    public async Task Visitantes_diferentes_somam_no_contador()
    {
        var post = await CriarPostAsync();

        await ClienteVisitante().PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);
        await ClienteVisitante().PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);

        Assert.Equal(2, await ContarLikesAsync(post.Id));
        Assert.Equal(2, await ContadorAsync(post.Id));
    }

    [Fact]
    public async Task Descurtir_remove_o_like_e_zera_o_contador()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();
        await leitor.PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);

        var resp = await leitor.DeleteAsync($"/api/v1/posts/{post.Id}/curtidas");

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        Assert.Equal(0, await ContarLikesAsync(post.Id));
        Assert.Equal(0, await ContadorAsync(post.Id));
    }

    [Fact]
    public async Task Descurtir_sem_ter_curtido_nao_deixa_o_contador_negativo()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        // Curtiu um visitante, descurtiu OUTRO: o contador não pode cair para -1 nem para 0.
        await ClienteVisitante().PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);
        var resp = await leitor.DeleteAsync($"/api/v1/posts/{post.Id}/curtidas");

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        Assert.Equal(1, await ContadorAsync(post.Id));
    }

    /// <summary>
    /// Issue #13: atrás de proxy o <c>RemoteIpAddress</c> é o do proxy para todo mundo. Sem
    /// <c>UseForwardedHeaders</c> dois leitores com o mesmo User-Agent viram o MESMO visitante —
    /// o segundo não conseguiria curtir, e o descurtir dele apagaria a curtida do primeiro.
    /// </summary>
    [Fact]
    public async Task Visitantes_atras_do_mesmo_proxy_nao_colapsam_na_mesma_identidade()
    {
        var post = await CriarPostAsync();
        var mesmoUserAgent = $"teste-{Guid.NewGuid():N}/1.0";
        var leitorA = ClienteAtrasDeProxy(mesmoUserAgent, "203.0.113.10");
        var leitorB = ClienteAtrasDeProxy(mesmoUserAgent, "203.0.113.11");

        await leitorA.PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);
        await leitorB.PostAsync($"/api/v1/posts/{post.Id}/curtidas", null);

        Assert.Equal(2, await ContarLikesAsync(post.Id));
        Assert.Equal(2, await ContadorAsync(post.Id));

        // E o descurtir de um não leva a curtida do outro junto.
        await leitorB.DeleteAsync($"/api/v1/posts/{post.Id}/curtidas");

        Assert.Equal(1, await ContarLikesAsync(post.Id));
    }

    /// <summary>
    /// O outro lado do #13: aceitar X-Forwarded-For de qualquer origem inverte o bug — o
    /// visitante passa a escolher a própria identidade e curte o mesmo post quantas vezes quiser.
    /// Vindo de um IP fora da allowlist, o header tem que ser ignorado.
    /// </summary>
    [Fact]
    public async Task Forjar_x_forwarded_for_de_origem_nao_confiavel_nao_cria_visitante()
    {
        var post = await CriarPostAsync();
        var mesmoUserAgent = $"teste-{Guid.NewGuid():N}/1.0";

        // Mesma origem (não confiável), dois X-Forwarded-For diferentes: uma curtida só.
        await CurtirForjandoAsync(post.Id, mesmoUserAgent, origem: "198.51.100.5", forjado: "203.0.113.20");
        await CurtirForjandoAsync(post.Id, mesmoUserAgent, origem: "198.51.100.5", forjado: "203.0.113.21");

        Assert.Equal(1, await ContarLikesAsync(post.Id));
        Assert.Equal(1, await ContadorAsync(post.Id));
    }

    [Fact]
    public async Task Curtir_post_inexistente_da_404()
    {
        var resp = await ClienteVisitante().PostAsync($"/api/v1/posts/{Guid.NewGuid()}/curtidas", null);

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    private async Task<int> ContarLikesAsync(Guid postId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        return await db.Likes.AsNoTracking().CountAsync(l => l.PostId == postId);
    }

    private async Task<int> ContadorAsync(Guid postId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        return await db.Posts.AsNoTracking().Where(p => p.Id == postId).Select(p => p.QtdCurtidas).FirstAsync();
    }

    /// <summary>Cliente anônimo com identidade de visitante única.</summary>
    private HttpClient ClienteVisitante()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"teste-{Guid.NewGuid():N}/1.0");
        return client;
    }

    /// <summary>Cliente que chega pelo proxy: mesmo User-Agent, IP real só no X-Forwarded-For.</summary>
    private HttpClient ClienteAtrasDeProxy(string userAgent, string ip)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);
        return client;
    }

    /// <summary>
    /// Curte montando o HttpContext direto no TestServer — é o único jeito de escolher o IP de
    /// origem (o <c>HttpClient</c> do factory não define <c>RemoteIpAddress</c>).
    /// </summary>
    private async Task CurtirForjandoAsync(Guid postId, string userAgent, string origem, string forjado)
    {
        await factory.Server.SendAsync(ctx =>
        {
            ctx.Request.Method = HttpMethods.Post;
            ctx.Request.Path = $"/api/v1/posts/{postId}/curtidas";
            ctx.Request.Headers.UserAgent = userAgent;
            ctx.Request.Headers["X-Forwarded-For"] = forjado;
            ctx.Connection.RemoteIpAddress = IPAddress.Parse(origem);
        });
    }

    private async Task<PostResponse> CriarPostAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var autor = await db.Usuarios.FirstAsync();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));

        var resp = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            titulo = $"Post pra curtir {Guid.NewGuid():N}",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new object[]
            {
                new { tipo = TipoBloco.Texto, ordem = 1, html = "<p>respirar fundo</p>" },
            },
        });

        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PostResponse>())!;
    }
}
