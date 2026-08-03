using PensaComigo.Application.Common;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.UnitTests.Common;

public class CalculadoraTempoLeituraTests
{
    private static Bloco Texto(string html) => new() { Tipo = TipoBloco.Texto, Html = html };
    private static Bloco Imagem() => new() { Tipo = TipoBloco.Imagem, ImagemPath = "posts/x.png" };
    private static string Palavras(int n) => string.Join(' ', Enumerable.Repeat("palavra", n));

    [Theory]
    [InlineData(1, 1)]      // post curtíssimo ainda é 1 minuto
    [InlineData(200, 1)]    // exatamente o limite
    [InlineData(201, 2)]    // 1 palavra a mais → arredonda pra cima
    [InlineData(600, 3)]
    public void Calcula_por_palavras_arredondando_pra_cima(int palavras, int esperado) =>
        Assert.Equal(esperado, CalculadoraTempoLeitura.Calcular([Texto(Palavras(palavras))]));

    [Fact]
    public void Soma_os_blocos_de_texto() =>
        Assert.Equal(2, CalculadoraTempoLeitura.Calcular(
            [Texto(Palavras(150)), Texto(Palavras(150))]));

    [Fact]
    public void Nao_conta_as_tags_html() =>
        Assert.Equal(1, CalculadoraTempoLeitura.Calcular(
            [Texto("<p><strong>Deus</strong> é <em>fiel</em></p>")]));

    [Fact]
    public void Post_so_de_imagem_tem_minimo_de_um_minuto() =>
        Assert.Equal(1, CalculadoraTempoLeitura.Calcular([Imagem()]));

    [Fact]
    public void Post_vazio_nao_quebra() =>
        Assert.Equal(1, CalculadoraTempoLeitura.Calcular([]));
}
