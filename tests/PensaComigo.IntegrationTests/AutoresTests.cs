using System.Net.Http.Json;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;

namespace PensaComigo.IntegrationTests;

public class AutoresTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Lista_autores_anonima_devolve_seed_com_bio()
    {
        var resp = await factory.CreateClient().GetAsync("/api/v1/autores");

        resp.EnsureSuccessStatusCode();
        var pagina = await resp.Content.ReadFromJsonAsync<Pagina<AutorResponse>>();

        Assert.True(pagina!.TotalItems >= 2);   // seed: Antonio e Jessica
        var antonio = Assert.Single(pagina.Items, a => a.Nome == "Antonio Ramon");
        Assert.False(string.IsNullOrWhiteSpace(antonio.Bio));
    }
}
