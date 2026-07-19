using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Prova que a fundação (Ticket 01) está de pé: a API sobe, a DI resolve o DbContext,
/// a migration aplica num Postgres real e o seed está lá. A partir daqui, cada fatia
/// de negócio ganha testes finos em cima deste harness.
/// </summary>
public class SpineSmokeTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Migration_aplica_e_seed_de_usuarios_esta_presente()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        var nomes = await db.Usuarios.Select(u => u.Nome).ToListAsync();

        Assert.Contains("Antonio Ramon", nomes);
        Assert.Contains("Jessica Rose", nomes);
    }
}
