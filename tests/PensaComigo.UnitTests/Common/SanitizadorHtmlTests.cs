using PensaComigo.Application.Common;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.UnitTests.Common;

public class SanitizadorHtmlTests
{
    [Fact]
    public void Script_some_por_inteiro()
    {
        var limpo = SanitizadorHtml.Sanitizar("<p>oi</p><script>alert('xss')</script>");

        Assert.DoesNotContain("script", limpo);
        Assert.DoesNotContain("alert", limpo);
        Assert.Contains("<p>oi</p>", limpo);
    }

    [Fact]
    public void Classe_fora_da_whitelist_some_e_tag_permitida_fica()
    {
        var limpo = SanitizadorHtml.Sanitizar("<p class=\"hackzona\">texto</p>");

        Assert.DoesNotContain("hackzona", limpo);
        Assert.Contains("<p>texto</p>", limpo);
    }

    [Fact]
    public void Href_javascript_some()
    {
        var limpo = SanitizadorHtml.Sanitizar("<a href=\"javascript:alert(1)\">clique</a>");

        Assert.DoesNotContain("javascript", limpo);
        Assert.Contains("clique", limpo);
    }

    [Fact]
    public void Href_https_e_title_sobrevivem()
    {
        var limpo = SanitizadorHtml.Sanitizar("<a href=\"https://ex.com/\" title=\"ex\">link</a>");

        Assert.Contains("href=\"https://ex.com/\"", limpo);
        Assert.Contains("title=\"ex\"", limpo);
    }

    [Fact]
    public void Versiculo_e_aside_sobrevivem_intactos()
    {
        const string verse = "<div class=\"verse\"><div class=\"r\">Sl 23.1</div><q>O Senhor é o meu pastor</q></div>";
        const string aside = "<div class=\"aside\"><b>Nota:</b> contexto histórico</div>";

        Assert.Equal(verse, SanitizadorHtml.Sanitizar(verse));
        Assert.Equal(aside, SanitizadorHtml.Sanitizar(aside));
    }

    [Theory]
    [InlineData("<p style=\"color:red\" id=\"x\" onclick=\"a()\">t</p>", "<p>t</p>")]
    [InlineData("<img src=\"x.png\">antes", "antes")]
    [InlineData("<iframe src=\"https://ex.com\"></iframe>fora", "fora")]
    [InlineData("<span>mantém o texto</span>", "mantém o texto")]
    public void Atributos_e_tags_proibidos_sao_removidos(string sujo, string esperado) =>
        Assert.Equal(esperado, SanitizadorHtml.Sanitizar(sujo));

    [Fact]
    public void SanitizarBlocos_so_mexe_no_html_de_blocos_texto()
    {
        var blocos = new List<Bloco>
        {
            new() { Tipo = TipoBloco.Texto, Ordem = 1, Html = "<p onclick=\"a()\">t</p>" },
            new() { Tipo = TipoBloco.Imagem, Ordem = 2, ImagemPath = "posts/x.png" },
        };

        SanitizadorHtml.SanitizarBlocos(blocos);

        Assert.Equal("<p>t</p>", blocos[0].Html);
        Assert.Equal("posts/x.png", blocos[1].ImagemPath);
    }
}
