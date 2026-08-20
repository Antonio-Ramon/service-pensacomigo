using PensaComigo.Application.Common;

namespace PensaComigo.UnitTests.Common;

public class ExtratorOpenGraphTests
{
    private const string Url = "https://exemplo.com/artigo";

    [Fact]
    public void Extrai_todas_as_tags_og()
    {
        const string html = """
            <html><head>
            <meta property="og:title" content="Título OG">
            <meta property="og:description" content="Descrição OG">
            <meta property="og:image" content="https://exemplo.com/capa.png">
            <meta property="og:site_name" content="Exemplo">
            <title>Título da aba</title>
            </head></html>
            """;

        var preview = ExtratorOpenGraph.Extrair(Url, html);

        Assert.Equal(Url, preview.Url);
        Assert.Equal("Título OG", preview.Titulo);
        Assert.Equal("Descrição OG", preview.Descricao);
        Assert.Equal("https://exemplo.com/capa.png", preview.Thumbnail);
        Assert.Equal("Exemplo", preview.SiteName);
    }

    [Fact]
    public void Sem_og_cai_no_title_e_na_meta_description()
    {
        const string html = """
            <html><head>
            <title>  Título da aba  </title>
            <meta name="description" content="Descrição clássica">
            </head></html>
            """;

        var preview = ExtratorOpenGraph.Extrair(Url, html);

        Assert.Equal("Título da aba", preview.Titulo);
        Assert.Equal("Descrição clássica", preview.Descricao);
        Assert.Null(preview.Thumbnail);
        Assert.Null(preview.SiteName);
    }

    [Fact]
    public void Html_sem_nada_devolve_tudo_nulo_menos_a_url()
    {
        var preview = ExtratorOpenGraph.Extrair(Url, "<html><body>oi</body></html>");

        Assert.Equal(Url, preview.Url);
        Assert.Null(preview.Titulo);
        Assert.Null(preview.Descricao);
        Assert.Null(preview.Thumbnail);
        Assert.Null(preview.SiteName);
    }
}
