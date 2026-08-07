using PensaComigo.Application.Common;

namespace PensaComigo.UnitTests.Common;

/// <summary>
/// O relógio é PARÂMETRO, não `DateTime.UtcNow` — por isso dá pra provar a janela de
/// 1 minuto sem esperar 1 minuto.
/// </summary>
public class JanelaDeslizanteTests
{
    private static readonly DateTime Agora = new(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Janela = TimeSpan.FromMinutes(1);

    [Fact]
    public void Primeiro_registro_passa_e_guarda_o_carimbo()
    {
        var carimbos = JanelaDeslizante.Registrar([], Agora, Janela, maximo: 5);

        Assert.Equal([Agora], carimbos);
    }

    [Fact]
    public void Abaixo_do_maximo_passa_e_acumula()
    {
        var anteriores = Enumerable.Range(1, 4).Select(i => Agora.AddSeconds(-i)).ToList();

        var carimbos = JanelaDeslizante.Registrar(anteriores, Agora, Janela, maximo: 5);

        Assert.Equal(5, carimbos!.Count);
    }

    [Fact]
    public void No_maximo_dentro_da_janela_estoura()
    {
        var anteriores = Enumerable.Range(1, 5).Select(i => Agora.AddSeconds(-i)).ToList();

        Assert.Null(JanelaDeslizante.Registrar(anteriores, Agora, Janela, maximo: 5));
    }

    [Fact]
    public void Carimbo_fora_da_janela_e_descartado_e_libera_vaga()
    {
        // 5 carimbos, mas o mais antigo tem 1min01 → só 4 valem.
        var anteriores = new List<DateTime>
        {
            Agora.AddSeconds(-61), Agora.AddSeconds(-4), Agora.AddSeconds(-3),
            Agora.AddSeconds(-2), Agora.AddSeconds(-1),
        };

        var carimbos = JanelaDeslizante.Registrar(anteriores, Agora, Janela, maximo: 5);

        Assert.Equal(5, carimbos!.Count);                       // 4 vigentes + o novo
        Assert.DoesNotContain(Agora.AddSeconds(-61), carimbos);
    }

    [Fact]
    public void Janela_desliza_em_vez_de_zerar_no_minuto_cheio()
    {
        var anteriores = Enumerable.Repeat(Agora, 5).ToList();   // 5 rajadas no mesmo instante

        // 59s depois ainda está cheio; 61s depois os 5 saíram todos da janela de uma vez.
        Assert.Null(JanelaDeslizante.Registrar(anteriores, Agora.AddSeconds(59), Janela, 5));
        Assert.NotNull(JanelaDeslizante.Registrar(anteriores, Agora.AddSeconds(61), Janela, 5));
    }
}
