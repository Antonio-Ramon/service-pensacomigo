using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Enums;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Ticket 07 (1ª metade): comentar, responder e a moderação automática.
/// <para>
/// Cada teste usa um <b>User-Agent próprio</b>: o hash do visitante nasce de
/// IP + User-Agent e o rate limit é por visitante — com um UA só, o 6º comentário
/// do arquivo inteiro tomaria 429 e os testes se contaminariam.
/// </para>
/// </summary>
public class ComentariosTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Comenta_anonimo_e_publica_na_hora()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        var resp = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Jéssica", conteudo = "Isso me acalmou hoje." });

        resp.EnsureSuccessStatusCode();
        var comentario = await resp.Content.ReadFromJsonAsync<ComentarioResponse>();

        Assert.True(comentario!.Aprovado);      // moderação automática: sem fila de aprovação
        Assert.Null(comentario.ParentId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var salvo = await db.Comentarios.AsNoTracking().FirstAsync(c => c.Id == comentario.Id);

        Assert.Equal(post.Id, salvo.PostId);
        Assert.True(salvo.Aprovado);
        Assert.NotEqual(default, salvo.DataCriacao);   // default now() da coluna
    }

    [Fact]
    public async Task Responde_comentario_raiz_mas_recusa_o_segundo_nivel()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();
        var raiz = await ComentarAsync(leitor, post.Id, "Comentário raiz");

        var resposta = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { parentId = raiz.Id, autor = "Antonio", conteudo = "Que bom que ajudou." });

        resposta.EnsureSuccessStatusCode();
        var filho = await resposta.Content.ReadFromJsonAsync<ComentarioResponse>();
        Assert.Equal(raiz.Id, filho!.ParentId);

        // 2º nível: o schema aceitaria (parent_id é livre), a regra de negócio não.
        var neto = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { parentId = filho.Id, autor = "Antonio", conteudo = "Resposta da resposta." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, neto.StatusCode);
    }

    [Fact]
    public async Task Responder_comentario_de_outro_post_devolve_422()
    {
        var postA = await CriarPostAsync();
        var postB = await CriarPostAsync();
        var leitor = ClienteVisitante();
        var noPostA = await ComentarAsync(leitor, postA.Id, "Sou do post A");

        var resp = await leitor.PostAsJsonAsync($"/api/v1/posts/{postB.Id}/comentarios",
            new { parentId = noPostA.Id, autor = "Intruso", conteudo = "Conversa trocada." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Comentar_em_post_inexistente_devolve_404()
    {
        var resp = await ClienteVisitante().PostAsJsonAsync(
            $"/api/v1/posts/{Guid.NewGuid()}/comentarios",
            new { autor = "Ninguém", conteudo = "Post fantasma." });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Palavrao_e_bloqueado_com_422_e_nao_entra_no_banco()
    {
        var post = await CriarPostAsync();

        var resp = await ClienteVisitante().PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Anônimo", conteudo = "que MERDA de texto" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        Assert.False(await db.Comentarios.AnyAsync(c => c.PostId == post.Id));
    }

    [Fact]
    public async Task Sexto_comentario_no_mesmo_minuto_devolve_429()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        for (var i = 1; i <= 5; i++)
            await ComentarAsync(leitor, post.Id, $"Comentário {i}");

        var sexto = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Afobado", conteudo = "Mais um!" });

        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);

        // Outro visitante (User-Agent diferente → outro hash) não é afetado.
        var outro = await ClienteVisitante().PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Calmo", conteudo = "Cheguei agora." });

        outro.EnsureSuccessStatusCode();
    }

    /// <summary>Cliente anônimo com identidade de visitante única (isola o rate limit).</summary>
    private HttpClient ClienteVisitante()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"teste-{Guid.NewGuid():N}/1.0");
        return client;
    }

    private static async Task<ComentarioResponse> ComentarAsync(HttpClient client, Guid postId, string texto)
    {
        var resp = await client.PostAsJsonAsync($"/api/v1/posts/{postId}/comentarios",
            new { autor = "Leitor", conteudo = texto });

        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ComentarioResponse>())!;
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
            titulo = $"Post pra comentar {Guid.NewGuid():N}",
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
