using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;
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

    [Fact]
    public async Task Edita_post_mantem_slug_troca_tags_e_recalcula_tempo()
    {
        var client = await ClienteAutenticadoAsync();
        var tagA = await CriarTagAsync(client, $"Calma {Guid.NewGuid():N}");
        var tagB = await CriarTagAsync(client, $"Foco {Guid.NewGuid():N}");
        var criado = await CriarPostAsync(client, "Voltar Ao Corpo", tagA, palavras: 100);

        var resp = await client.PutAsJsonAsync($"/api/v1/posts/{criado.Id}", new
        {
            titulo = "Outro Título Completamente Diferente",
            imagemCapa = "posts/nova.webp",
            tagIds = new[] { tagB },
            conteudo = new object[]
            {
                new { tipo = TipoBloco.Texto, ordem = 1, html = Texto(600) },
            },
        });

        resp.EnsureSuccessStatusCode();
        var editado = await resp.Content.ReadFromJsonAsync<PostResponse>();

        Assert.Equal(criado.Slug, editado!.Slug);   // slug congelado, mesmo com título novo
        Assert.Equal(3, editado.TempoLeitura);      // 600 palavras / 200 ppm

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var salvo = await db.Posts.Include(p => p.Tags).AsNoTracking().FirstAsync(p => p.Id == criado.Id);

        Assert.Equal("posts/nova.webp", salvo.ImagemCapa);
        Assert.Single(salvo.Conteudo);
        Assert.Equal(tagB, Assert.Single(salvo.Tags).Id);   // junção trocada, não somada
    }

    [Fact]
    public async Task Editar_post_de_outro_autor_devolve_404()
    {
        var dono = await ClienteAutenticadoAsync();
        var intruso = await ClienteAutenticadoAsync("Jessica");
        var post = await CriarPostAsync(dono, $"Post do dono {Guid.NewGuid():N}");

        var resp = await intruso.PutAsJsonAsync($"/api/v1/posts/{post.Id}", new
        {
            titulo = "Sequestrado",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new object[] { new { tipo = TipoBloco.Texto, ordem = 1, html = Texto(10) } },
        });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Deleta_post_do_proprio_autor_e_some_do_banco()
    {
        var client = await ClienteAutenticadoAsync();
        var tag = await CriarTagAsync(client, $"Silêncio {Guid.NewGuid():N}");
        var post = await CriarPostAsync(client, $"Para apagar {Guid.NewGuid():N}", tag);

        var resp = await client.DeleteAsync($"/api/v1/posts/{post.Id}");

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        Assert.False(await db.Posts.AnyAsync(p => p.Id == post.Id));
        Assert.True(await db.Tags.AnyAsync(t => t.Id == tag));   // cascata leva a junção, não a tag
    }

    [Fact]
    public async Task Abre_post_por_slug_anonimo_com_autor_tags_e_incrementa_visualizacoes()
    {
        var autor = await ClienteAutenticadoAsync();
        var tag = await CriarTagAsync(autor, $"Presença {Guid.NewGuid():N}");
        var criado = await CriarPostAsync(autor, $"Abrir e contar {Guid.NewGuid():N}", tag);

        var anonimo = factory.CreateClient();   // sem token: leitura é pública
        var resp = await anonimo.GetAsync($"/api/v1/posts/{criado.Slug}");

        resp.EnsureSuccessStatusCode();
        var post = await resp.Content.ReadFromJsonAsync<PostDetalheResponse>();

        Assert.Equal(criado.Id, post!.Id);
        Assert.Equal("Antonio Ramon", post.Autor.Nome);
        Assert.Equal(tag, Assert.Single(post.Tags).Id);
        Assert.Single(post.Conteudo);
        Assert.Equal(1, post.QtdVisualizacoes);   // primeira abertura

        await anonimo.GetAsync($"/api/v1/posts/{criado.Slug}");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        Assert.Equal(2, (await db.Posts.AsNoTracking().FirstAsync(p => p.Id == criado.Id)).QtdVisualizacoes);
    }

    [Fact]
    public async Task Abrir_slug_inexistente_devolve_404()
    {
        var resp = await factory.CreateClient().GetAsync($"/api/v1/posts/nao-existe-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Lista_posts_anonima_devolve_envelope_e_respeita_filtro_por_tag()
    {
        var autor = await ClienteAutenticadoAsync();
        var nomeTag = $"Feed {Guid.NewGuid():N}";
        var tag = await CriarTagAsync(autor, nomeTag);
        var slugTag = (await ListarTagAsync(nomeTag)).Slug;
        var meu = await CriarPostAsync(autor, $"Só este tem a tag {Guid.NewGuid():N}", tag);
        await CriarPostAsync(autor, $"Este não tem {Guid.NewGuid():N}");

        var resp = await factory.CreateClient().GetAsync($"/api/v1/posts?filter=tag={slugTag}");

        resp.EnsureSuccessStatusCode();
        var pagina = await resp.Content.ReadFromJsonAsync<Pagina<PostResumoResponse>>();

        Assert.Equal(1, pagina!.TotalItems);
        var item = Assert.Single(pagina.Items);
        Assert.Equal(meu.Id, item.Id);
        Assert.Equal("Antonio Ramon", item.Autor.Nome);   // card da home: autor + tags no resumo
        Assert.Equal(tag, Assert.Single(item.Tags).Id);
    }

    [Fact]
    public async Task Lista_posts_ordena_por_data_desc_por_padrao()
    {
        var autor = await ClienteAutenticadoAsync();
        var marca = $"{Guid.NewGuid():N}";   // isola estes dois posts do resto do banco
        await CriarPostAsync(autor, $"Antigo {marca}");
        var novo = await CriarPostAsync(autor, $"Recente {marca}");

        var resp = await factory.CreateClient().GetAsync($"/api/v1/posts?filter=titulo=*{marca}");

        resp.EnsureSuccessStatusCode();
        var pagina = await resp.Content.ReadFromJsonAsync<Pagina<PostResumoResponse>>();

        Assert.Equal(2, pagina!.TotalItems);
        Assert.Equal(novo.Id, pagina.Items[0].Id);   // sem OrderBy na querystring → data desc
    }

    private async Task<Domain.Entities.Tag> ListarTagAsync(string nome)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        return await db.Tags.AsNoTracking().FirstAsync(t => t.Nome == nome);
    }

    [Fact]
    public async Task Rascunho_nao_aparece_no_feed_anonimo_nem_abre_por_slug()
    {
        var autor = await ClienteAutenticadoAsync();
        var marca = $"{Guid.NewGuid():N}";
        var rascunho = await CriarPostAsync(autor, $"Rascunho {marca}", status: 0);

        var anonimo = factory.CreateClient();

        var feed = await (await anonimo.GetAsync($"/api/v1/posts?filter=titulo=*{marca}"))
            .Content.ReadFromJsonAsync<Pagina<PostResumoResponse>>();
        Assert.Equal(0, feed!.TotalItems);

        Assert.Equal(HttpStatusCode.NotFound,
            (await anonimo.GetAsync($"/api/v1/posts/{rascunho.Slug}")).StatusCode);

        // Autor logado vê o rascunho na listagem, com o status no DTO.
        var doAutor = await (await autor.GetAsync($"/api/v1/posts?filter=titulo=*{marca}"))
            .Content.ReadFromJsonAsync<Pagina<PostResumoResponse>>();
        var item = Assert.Single(doAutor!.Items);
        Assert.Equal(Domain.Enums.StatusPost.Rascunho, item.Status);
        Assert.Null(item.DataPublicacao);
    }

    [Fact]
    public async Task Publicar_rascunho_carimba_DataPublicacao_e_republicar_nao_muda()
    {
        var autor = await ClienteAutenticadoAsync();
        var rascunho = await CriarPostAsync(autor, $"Publicar depois {Guid.NewGuid():N}", status: 0);

        var corpo = new
        {
            titulo = "Publicado agora",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new object[] { new { tipo = TipoBloco.Texto, ordem = 1, html = Texto(10) } },
            status = 1,
        };
        (await autor.PutAsJsonAsync($"/api/v1/posts/{rascunho.Id}", corpo)).EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var publicadoEm = (await db.Posts.AsNoTracking().FirstAsync(p => p.Id == rascunho.Id)).DataPublicacao;
        Assert.NotNull(publicadoEm);

        // Republicar (novo PUT com status=1) não reposiciona o post no feed.
        (await autor.PutAsJsonAsync($"/api/v1/posts/{rascunho.Id}", corpo)).EnsureSuccessStatusCode();
        var depois = (await db.Posts.AsNoTracking().FirstAsync(p => p.Id == rascunho.Id)).DataPublicacao;
        Assert.Equal(publicadoEm, depois);
    }

    private static string Texto(int palavras) =>
        "<p>" + string.Join(" ", Enumerable.Repeat("palavra", palavras)) + "</p>";

    private static async Task<PostResponse> CriarPostAsync(
        HttpClient client, string titulo, Guid? tagId = null, int palavras = 10, int status = 1)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            titulo,
            imagemCapa = "posts/capa.webp",
            tagIds = tagId is null ? Array.Empty<Guid>() : [tagId.Value],
            conteudo = new object[] { new { tipo = TipoBloco.Texto, ordem = 1, html = Texto(palavras) } },
            status,
        });

        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PostResponse>())!;
    }

    private async Task<Guid> CriarTagAsync(HttpClient client, string nome)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/tags", new { nome });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Application.Tags.TagResponse>())!.Id;
    }

    private async Task<HttpClient> ClienteAutenticadoAsync(string nome = "Antonio Ramon")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var autor = await db.Usuarios.FirstAsync(u => u.Nome.StartsWith(nome));

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));
        return client;
    }
}
