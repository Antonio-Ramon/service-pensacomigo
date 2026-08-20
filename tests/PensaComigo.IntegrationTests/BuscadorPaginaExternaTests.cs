using System.Net;
using PensaComigo.Domain.Exceptions;
using PensaComigo.Web.Links;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Guarda de SSRF do preview (issue #21) no nível do loop de redirects — sem rede: o
/// HttpMessageHandler é fake e o "host" inicial é um IP público literal (nenhum DNS real).
/// </summary>
public class BuscadorPaginaExternaTests
{
    [Fact]
    public async Task Redirect_para_ip_privado_e_bloqueado()
    {
        var buscador = Buscador(_ => Redirect("http://127.0.0.1/admin"));

        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => buscador.BaixarHtmlAsync("http://8.8.8.8/pagina"));

        Assert.Contains("interno", ex.Message);
    }

    [Fact]
    public async Task Mais_de_tres_redirects_e_bloqueado()
    {
        var buscador = Buscador(_ => Redirect("http://8.8.8.8/de-novo"));

        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => buscador.BaixarHtmlAsync("http://8.8.8.8/comeco"));
    }

    [Fact]
    public async Task Url_direta_para_ip_privado_e_bloqueada()
    {
        var buscador = Buscador(_ => throw new InvalidOperationException("não deveria nem chamar HTTP"));

        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => buscador.BaixarHtmlAsync("http://192.168.0.1/"));
    }

    [Fact]
    public async Task Content_type_que_nao_e_html_e_rejeitado()
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
        };
        var buscador = Buscador(_ => resp);

        await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => buscador.BaixarHtmlAsync("http://8.8.8.8/api"));
    }

    private static HttpResponseMessage Redirect(string destino)
    {
        var resp = new HttpResponseMessage(HttpStatusCode.Found);
        resp.Headers.Location = new Uri(destino);
        return resp;
    }

    private static BuscadorPaginaExterna Buscador(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new FabricaFake(new HandlerFake(responder)));

    private sealed class HandlerFake(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(responder(request));
    }

    private sealed class FabricaFake(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler);
    }
}
