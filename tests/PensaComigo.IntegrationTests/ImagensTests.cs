using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Imagens.UrlUpload;
using PensaComigo.Application.Storage;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Ticket 04: o endpoint exige JWT e devolve URL assinada. O seam IStorage é trocado por um
/// fake — o teste prova a nossa parte (auth, path montado no servidor, validação), não o Supabase.
/// </summary>
public class ImagensTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    private const string Rota = "/api/v1/imagens/url-upload";

    [Fact]
    public async Task Sem_token_devolve_401()
    {
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync(Rota, new { nomeArquivo = "capa.png" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Autor_recebe_url_assinada_com_path_da_propria_pasta()
    {
        var (client, autorId) = await ClienteAutenticadoAsync();

        var resp = await client.PostAsJsonAsync(Rota, new { nomeArquivo = "Minha Foto.PNG" });

        resp.EnsureSuccessStatusCode();
        var corpo = await resp.Content.ReadFromJsonAsync<UrlUploadResponse>();
        // Path montado pelo servidor: pasta do autor da claim + nome novo + extensão normalizada.
        Assert.StartsWith($"posts/{autorId}/", corpo!.Path);
        Assert.EndsWith(".png", corpo.Path);
        Assert.Equal($"{StorageFake.Prefixo}{corpo.Path}", corpo.UrlAssinada);
    }

    [Fact]
    public async Task Extensao_nao_permitida_devolve_422()
    {
        var (client, _) = await ClienteAutenticadoAsync();

        var resp = await client.PostAsJsonAsync(Rota, new { nomeArquivo = "payload.svg" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    // Cliente com token do autor do seed e o IStorage real trocado pelo fake.
    private async Task<(HttpClient Client, Guid AutorId)> ClienteAutenticadoAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var autor = await db.Usuarios.FirstAsync(u => u.Nome == "Antonio Ramon");

        // ConfigureTestServices roda DEPOIS do Program.cs → esta registração vence a do typed client.
        var client = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<IStorage, StorageFake>()))
            .CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));

        return (client, autor.Id);
    }

    private sealed class StorageFake : IStorage
    {
        public const string Prefixo = "https://fake.storage/assinada/";

        public Task<string> GerarUrlUploadAssinadaAsync(string path, CancellationToken ct) =>
            Task.FromResult(Prefixo + path);
    }
}
