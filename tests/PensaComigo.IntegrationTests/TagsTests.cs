using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Tags;
using PensaComigo.Domain.Common;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Ticket 03: criar tag exige JWT; a listagem é pública. Testa o trem inteiro
/// (controller → MediatR → handler → Postgres real) em cima do harness da Fatia 8.
/// </summary>
public class TagsTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Criar_sem_token_devolve_401()
    {
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/v1/tags", new { nome = "Ansiedade" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Autor_cria_e_leitor_anonimo_lista()
    {
        var autenticado = factory.CreateClient();
        autenticado.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await TokenDoSeedAsync());

        // Autor cria (acento vira slug limpo).
        var criada = await autenticado.PostAsJsonAsync("/api/v1/tags", new { nome = "Saúde Mental" });
        criada.EnsureSuccessStatusCode();
        var tag = await criada.Content.ReadFromJsonAsync<TagResponse>();
        Assert.Equal("saude-mental", tag!.Slug);

        // Leitor anônimo enxerga a tag recém-criada — resposta no envelope { items, totalItems }.
        var anonimo = factory.CreateClient();
        var pagina = await anonimo.GetFromJsonAsync<Pagina<TagResponse>>("/api/v1/tags");
        Assert.Contains(pagina!.Items, t => t.Slug == "saude-mental");
        Assert.True(pagina.TotalItems >= 1);

        // Filtro dinâmico do Gridify (DSL na querystring, traduzida pra SQL).
        var filtrada = await anonimo.GetFromJsonAsync<Pagina<TagResponse>>("/api/v1/tags?filter=slug=saude-mental");
        Assert.Equal(1, filtrada!.TotalItems);
        Assert.Equal("saude-mental", filtrada.Items.Single().Slug);
    }

    // Reusa o próprio gerador de JWT da API (via DI) e o usuário do seed pra emitir um token válido.
    private async Task<string> TokenDoSeedAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var autor = await db.Usuarios.FirstAsync(u => u.Nome == "Antonio Ramon");
        return jwt.Gerar(autor);
    }
}
