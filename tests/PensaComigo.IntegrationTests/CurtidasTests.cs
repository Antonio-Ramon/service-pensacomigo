using System.Net;
using System.Net.Http.Headers;
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
