using System.Net;
using System.Net.Http.Json;
using PensaComigo.Shared.Erros;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// O contrato de ERRO é público como qualquer endpoint: o front depende do formato.
/// Estes testes olham o CORPO, não só o status — foi exatamente aí que dois bugs se
/// esconderam (resposta sobrescrita pelo ProblemDetails genérico, e detail genérico).
/// </summary>
public class ErrosTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Sem_token_devolve_401_no_envelope()
    {
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/posts", new { titulo = "x" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        var erro = await LerAsync(resp);
        Assert.False(erro.Successed);
        Assert.NotEmpty(erro.Message);
        Assert.Equal("Erro", Assert.Single(erro.Notifications).Key);
    }

    [Fact]
    public async Task Validacao_devolve_422_com_uma_notificacao_por_campo()
    {
        var resp = await factory.CreateClient()
            .PostAsJsonAsync("/api/v1/auth/login", new { });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
        var erro = await LerAsync(resp);

        // Key = nome do campo no MESMO camelCase que o cliente enviou.
        var falha = Assert.Single(erro.Notifications);
        Assert.Equal("idToken", falha.Key);
        Assert.Equal("O token do Google é obrigatório.", falha.Message);
        // Message repete as mensagens: é a frase que o front joga no toast.
        Assert.Contains("token do Google", erro.Message);
    }

    [Fact]
    public async Task Json_quebrado_devolve_400_no_mesmo_envelope()
    {
        var corpo = new StringContent("{titulo:", System.Text.Encoding.UTF8, "application/json");

        var resp = await factory.CreateClient().PostAsync("/api/v1/auth/login", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var erro = await LerAsync(resp);
        Assert.False(erro.Successed);
        Assert.NotEmpty(erro.Notifications);
    }

    [Fact]
    public async Task Erro_de_cliente_nao_vaza_stack_trace()
    {
        // A factory roda em Development; mesmo assim 4xx não carrega `debug`.
        var resp = await factory.CreateClient().PostAsJsonAsync("/api/v1/auth/login", new { });

        Assert.Null((await LerAsync(resp)).Debug);
    }

    private static async Task<RespostaErro> LerAsync(HttpResponseMessage resp) =>
        (await resp.Content.ReadFromJsonAsync<RespostaErro>())!;
}
