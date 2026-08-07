using PensaComigo.Application.Common;

namespace PensaComigo.UnitTests.Common;

public class FiltroPalavraoTests
{
    [Theory]
    [InlineData("que merda de post")]
    [InlineData("QUE MERDA")]              // caixa não importa
    [InlineData("que MeRdA!")]             // pontuação colada também não
    [InlineData("desgraça de texto")]      // acento é normalizado antes de comparar
    [InlineData("seu idiota")]
    public void Reprova_texto_com_termo_proibido(string texto) =>
        Assert.True(FiltroPalavrao.Contem(texto));

    [Theory]
    [InlineData("adorei a meditação, obrigado")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Aprova_texto_limpo(string? texto) =>
        Assert.False(FiltroPalavrao.Contem(texto));

    [Fact]
    public void Compara_palavra_inteira_e_nao_substring()
    {
        // "cu" está na lista; se a comparação fosse por substring, "curso" reprovaria.
        Assert.False(FiltroPalavrao.Contem("fiz um curso de meditação"));
        Assert.True(FiltroPalavrao.Contem("que cu"));
    }
}
