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
/// Ticket 05: criar post. O que só o Postgres real prova — o jsonb do conteúdo,
/// as linhas de junção post_tags e o índice único do slug.
/// </summary>
public class PostsTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Criar_sem_token_devolve_401()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/posts", new { titulo = "Silêncio" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Cria_post_com_tag_grava_jsonb_e_resolve_colisao_de_slug()
    {
        var client = await ClienteAutenticadoAsync();
        var tagId = await CriarTagAsync(client, $"Meditação {Guid.NewGuid():N}");

        var corpo = new
        {
            titulo = "Respirar Devagar",
            imagemCapa = "posts/capa.webp",
            tagIds = new[] { tagId },
            conteudo = new object[]
            {
                new { tipo = TipoBloco.Texto, ordem = 1, html = "<p>" + string.Join(" ", Enumerable.Repeat("palavra", 250)) + "</p>" },
                new { tipo = TipoBloco.Imagem, ordem = 2, imagemPath = "posts/foto.webp", alt = "respiração" },
            },
        };

        var resp = await client.PostAsJsonAsync("/api/v1/posts", corpo);
        resp.EnsureSuccessStatusCode();
        var post = await resp.Content.ReadFromJsonAsync<PostResponse>();

        Assert.Equal("respirar-devagar", post!.Slug);
        Assert.Equal(2, post.TempoLeitura); // 250 palavras / 200 ppm, arredondando pra cima

        // Mesmo título de novo: o slug congelado do primeiro força o sufixo no segundo.
        var segundo = await client.PostAsJsonAsync("/api/v1/posts", corpo);
        segundo.EnsureSuccessStatusCode();
        Assert.Equal("respirar-devagar-2", (await segundo.Content.ReadFromJsonAsync<PostResponse>())!.Slug);

        // Ida ao banco: o jsonb volta como List<Bloco> e a tag ficou vinculada (sem duplicar a tag).
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var salvo = await db.Posts.Include(p => p.Tags).AsNoTracking().FirstAsync(p => p.Id == post.Id);

        Assert.Equal(2, salvo.Conteudo.Count);
        Assert.Equal(TipoBloco.Imagem, salvo.Conteudo[1].Tipo);
        Assert.Equal("posts/foto.webp", salvo.Conteudo[1].ImagemPath);
        Assert.Equal(tagId, Assert.Single(salvo.Tags).Id);
        Assert.Equal(1, await db.Tags.CountAsync(t => t.Id == tagId));
    }

    [Fact]
    public async Task Bloco_de_texto_sem_html_devolve_422()
    {
        var client = await ClienteAutenticadoAsync();

        var resp = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            titulo = "Bloco vazio",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new[] { new { tipo = TipoBloco.Texto, ordem = 1 } },
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    private async Task<Guid> CriarTagAsync(HttpClient client, string nome)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/tags", new { nome });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Application.Tags.TagResponse>())!.Id;
    }

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var autor = await db.Usuarios.FirstAsync(u => u.Nome == "Antonio Ramon");

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));
        return client;
    }
}
