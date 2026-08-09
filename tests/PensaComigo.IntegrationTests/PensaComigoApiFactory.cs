using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Persistence;
using Testcontainers.PostgreSql;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Sobe a API em memória (WebApplicationFactory) apontada para um Postgres real
/// e descartável (Testcontainers). A migration inicial é aplicada no start.
/// </summary>
public sealed class PensaComigoApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    // Roda o Program.cs, mas troca a connection string "Default" pela do container.
    // AddDbContext lê GetConnectionString no momento de construir o DbContext, então
    // basta sobrescrever a setting antes do build — sem mexer na DI de produção.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        // Chave de teste pra o JwtTokenGenerator emitir tokens que o AddJwtBearer valida.
        // (>= 32 bytes p/ HS256). Em produção vem de user-secrets.
        builder.UseSetting("Jwt:Key", "chave-de-teste-com-mais-de-32-bytes-para-hs256!!");
        // SupabaseOptions é validada no start (ValidateOnStart) — sem isso a app nem sobe no teste.
        // Valores de fachada: quem chama o Supabase de verdade é trocado por fake em ImagensTests.
        // Segredo do hash do visitante (HMAC), >= 32 chars pela validação de options.
        builder.UseSetting("Visitantes:Pepper", "pepper-de-teste-com-32-caracteres!");
        // Proxy confiável dos testes. O TestServer não define RemoteIpAddress (null), então o
        // primeiro hop passa; quem quiser exercitar a allowlist define o IP de origem na mão
        // (ver CurtidasTests.Forjar_x_forwarded_for_de_origem_nao_confiavel_nao_cria_visitante).
        builder.UseSetting("ProxiesConfiaveis:0", "127.0.0.1");
        builder.UseSetting("ProxiesConfiaveis:1", "10.0.0.0/8");
        builder.UseSetting("Supabase:Url", "https://teste.supabase.co");
        builder.UseSetting("Supabase:ServiceRoleKey", "chave-de-teste");
    }

    // Explícito: o IAsyncLifetime do xunit v2 usa Task; a base WebApplicationFactory
    // já tem um DisposeAsync (ValueTask). Implementar a interface explicitamente evita
    // o choque de assinaturas entre os dois DisposeAsync.
    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
