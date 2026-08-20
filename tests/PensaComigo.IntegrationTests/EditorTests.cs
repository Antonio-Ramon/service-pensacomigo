using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Etapas;
using PensaComigo.Application.Posts;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Enums;
using PensaComigo.Persistence;
using PensaComigo.Shared.Erros;

namespace PensaComigo.IntegrationTests;

/// <summary>Issues #26–#32: dek, moods, etapas, agendamento, GET por id sem view e DELETE de tag.</summary>
public class EditorTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Etapas_sao_publicas_e_vem_ordenadas()
    {
        var etapas = await factory.CreateClient()
            .GetFromJsonAsync<List<EtapaResponse>>("/api/v1/etapas");

        Assert.Equal(4, etapas!.Count);
        Assert.Equal([1, 2, 3, 4], etapas.Select(e => e.Numero));
    }

    [Fact]
    public async Task Dek_moods_e_etapa_persistem_e_saem_no_detalhe_por_id_sem_contar_view()
    {
        var client = await ClienteAutenticadoAsync();
        var etapa = (await client.GetFromJsonAsync<List<EtapaResponse>>("/api/v1/etapas"))![0];

        var criado = await CriarPostAsync(client, $"Com dek {Guid.NewGuid():N}", corpoExtra: new
        {
            dek = "Uma frase curta que resume sem entregar.",
            moods = new[] { Mood.Grato, Mood.Cansado },
            etapaId = etapa.Id,
        });

        // GET por id (autenticado): devolve os campos novos e NÃO conta visualização.
        var detalhe = await client.GetFromJsonAsync<PostDetalheResponse>($"/api/v1/posts/id/{criado.Id}");
        Assert.Equal("Uma frase curta que resume sem entregar.", detalhe!.Dek);
        Assert.Equal(2, detalhe.Moods.Count);
        Assert.Equal(etapa.Id, detalhe.Etapa!.Id);
        Assert.Equal(0, detalhe.QtdVisualizacoes);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        Assert.Equal(0, (await db.Posts.AsNoTracking().FirstAsync(p => p.Id == criado.Id)).QtdVisualizacoes);

        // Sem token → 401; o feed continua trazendo o dek no resumo.
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().GetAsync($"/api/v1/posts/id/{criado.Id}")).StatusCode);
    }

    [Fact]
    public async Task Filtro_por_mood_e_busca_sem_acento_no_titulo()
    {
        var client = await ClienteAutenticadoAsync();
        var marca = Guid.NewGuid().ToString("N")[..8];
        await CriarPostAsync(client, $"Oração Guiada {marca}", corpoExtra: new { moods = new[] { Mood.EmLuto } });

        var anonimo = factory.CreateClient();

        // Termo sem acento acha título com acento (e o /i segue valendo).
        var busca = await anonimo.GetFromJsonAsync<Pagina<PostResumoResponse>>(
            $"/api/v1/posts?filter=titulo=*oracao guiada {marca}/i");
        Assert.Equal(1, busca!.TotalItems);

        var porMood = await anonimo.GetFromJsonAsync<Pagina<PostResumoResponse>>(
            $"/api/v1/posts?filter=mood={(int)Mood.EmLuto}");
        Assert.Contains(porMood!.Items, p => p.Titulo.Contains(marca));
    }

    [Fact]
    public async Task Agendado_futuro_nao_aparece_pro_publico_mas_aparece_pro_autor()
    {
        var client = await ClienteAutenticadoAsync();
        var marca = Guid.NewGuid().ToString("N")[..8];
        var criado = await CriarPostAsync(client, $"Agendado {marca}", corpoExtra: new
        {
            status = StatusPost.Agendado,
            dataPublicacao = DateTime.UtcNow.AddDays(1),
        });

        var doPublico = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<PostResumoResponse>>($"/api/v1/posts?filter=titulo=*{marca}");
        Assert.Equal(0, doPublico!.TotalItems);

        // Slug direto também é 404 pro público enquanto a hora não chega.
        Assert.Equal(HttpStatusCode.NotFound,
            (await factory.CreateClient().GetAsync($"/api/v1/posts/{criado.Slug}")).StatusCode);

        var doAutor = await client
            .GetFromJsonAsync<Pagina<PostResumoResponse>>($"/api/v1/posts?filter=titulo=*{marca}");
        Assert.Equal(1, doAutor!.TotalItems);
        Assert.Equal(StatusPost.Agendado, doAutor.Items.Single().Status);
    }

    [Fact]
    public async Task Agendar_sem_data_futura_devolve_422()
    {
        var client = await ClienteAutenticadoAsync();

        var resp = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            titulo = "Agendado sem data",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new object[] { new { tipo = TipoBloco.Texto, ordem = 1, html = "<p>oi</p>" } },
            status = StatusPost.Agendado,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Deletar_tag_livre_remove_e_tag_em_uso_bloqueia_com_422()
    {
        var client = await ClienteAutenticadoAsync();

        // Sem token → 401.
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().DeleteAsync($"/api/v1/tags/{Guid.NewGuid()}")).StatusCode);

        // Tag livre: 204 e some da listagem.
        var livre = await CriarTagAsync(client, $"Descartável {Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/tags/{livre}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/v1/tags/{livre}")).StatusCode);

        // Tag vinculada a post: bloqueia com 422 e informa a contagem.
        var emUso = await CriarTagAsync(client, $"Em uso {Guid.NewGuid():N}");
        await CriarPostAsync(client, $"Post que usa a tag {Guid.NewGuid():N}", tagId: emUso);

        var bloqueio = await client.DeleteAsync($"/api/v1/tags/{emUso}");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, bloqueio.StatusCode);
        var erro = await bloqueio.Content.ReadFromJsonAsync<RespostaErro>();
        Assert.Contains("1 post", erro!.Message);
    }

    // ---- helpers ----

    private static async Task<PostResponse> CriarPostAsync(
        HttpClient client, string titulo, Guid? tagId = null, object? corpoExtra = null)
    {
        var basico = new Dictionary<string, object?>
        {
            ["titulo"] = titulo,
            ["imagemCapa"] = "posts/capa.webp",
            ["tagIds"] = tagId is null ? Array.Empty<Guid>() : new[] { tagId.Value },
            ["conteudo"] = new object[] { new { tipo = TipoBloco.Texto, ordem = 1, html = "<p>texto</p>" } },
            ["status"] = StatusPost.Publicado,
        };

        // Campos extras (dek, moods, etapaId, status, dataPublicacao) sobrescrevem o básico.
        if (corpoExtra is not null)
            foreach (var prop in corpoExtra.GetType().GetProperties())
                basico[char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..]] = prop.GetValue(corpoExtra);

        var resp = await client.PostAsJsonAsync("/api/v1/posts", basico);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PostResponse>())!;
    }

    private async Task<Guid> CriarTagAsync(HttpClient client, string nome)
    {
        var resp = await client.PostAsJsonAsync("/api/v1/tags", new { nome });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<TagResponse>())!.Id;
    }

    private async Task<HttpClient> ClienteAutenticadoAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var autor = await db.Usuarios.FirstAsync(u => u.Nome.StartsWith("Antonio Ramon"));

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));
        return client;
    }
}
