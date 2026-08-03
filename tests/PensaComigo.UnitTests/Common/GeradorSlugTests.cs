using PensaComigo.Application.Common;

namespace PensaComigo.UnitTests.Common;

public class GeradorSlugTests
{
    [Theory]
    [InlineData("Saúde Mental", "saude-mental")]
    [InlineData("  Fé, Esperança e Amor!  ", "fe-esperanca-e-amor")]
    [InlineData("Salmo 23", "salmo-23")]
    [InlineData("Oração   com    espaços", "oracao-com-espacos")]
    [InlineData("---Coração---", "coracao")]
    public void Gerar_normaliza_o_texto(string entrada, string esperado) =>
        Assert.Equal(esperado, GeradorSlug.Gerar(entrada));

    [Fact]
    public void Gerar_e_estavel_para_variacoes_do_mesmo_titulo() =>
        Assert.Equal(GeradorSlug.Gerar("Café da manhã"), GeradorSlug.Gerar("CAFÉ DA MANHÃ!"));

    [Fact]
    public void ResolverColisao_devolve_o_base_quando_esta_livre() =>
        Assert.Equal("salmo-23", GeradorSlug.ResolverColisao("salmo-23", ["outro-post"]));

    [Fact]
    public void ResolverColisao_sufixa_a_partir_do_2() =>
        Assert.Equal("salmo-23-2", GeradorSlug.ResolverColisao("salmo-23", ["salmo-23"]));

    [Fact]
    public void ResolverColisao_pula_sufixos_ja_ocupados() =>
        Assert.Equal("salmo-23-4",
            GeradorSlug.ResolverColisao("salmo-23", ["salmo-23", "salmo-23-2", "salmo-23-3"]));

    [Fact]
    public void ResolverColisao_ignora_slug_parecido_que_nao_e_sufixo() =>
        Assert.Equal("salmo-23", GeradorSlug.ResolverColisao("salmo-23", ["salmo-231"]));
}
