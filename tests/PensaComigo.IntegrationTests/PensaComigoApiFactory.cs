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
