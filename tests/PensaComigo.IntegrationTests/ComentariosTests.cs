using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Comentarios;
using PensaComigo.Application.Comentarios.Listar;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;
using PensaComigo.Domain.Entities;
using PensaComigo.Domain.Enums;
using PensaComigo.Persistence;

namespace PensaComigo.IntegrationTests;

/// <summary>
/// Ticket 07 (1ª metade): comentar, responder e a moderação automática.
/// <para>
/// Cada teste usa um <b>User-Agent próprio</b>: o hash do visitante nasce de
/// IP + User-Agent e o rate limit é por visitante — com um UA só, o 6º comentário
/// do arquivo inteiro tomaria 429 e os testes se contaminariam.
/// </para>
/// </summary>
public class ComentariosTests(PensaComigoApiFactory factory) : IClassFixture<PensaComigoApiFactory>
{
    [Fact]
    public async Task Comenta_anonimo_e_publica_na_hora()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        var resp = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Jéssica", conteudo = "Isso me acalmou hoje." });

        resp.EnsureSuccessStatusCode();
        var comentario = await resp.Content.ReadFromJsonAsync<ComentarioResponse>();

        Assert.True(comentario!.Aprovado);      // moderação automática: sem fila de aprovação
        Assert.Null(comentario.ParentId);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var salvo = await db.Comentarios.AsNoTracking().FirstAsync(c => c.Id == comentario.Id);

        Assert.Equal(post.Id, salvo.PostId);
        Assert.True(salvo.Aprovado);
        Assert.NotEqual(default, salvo.DataCriacao);   // default now() da coluna
    }

    [Fact]
    public async Task Responde_comentario_raiz_mas_recusa_o_segundo_nivel()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();
        var raiz = await ComentarAsync(leitor, post.Id, "Comentário raiz");

        var resposta = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { parentId = raiz.Id, autor = "Antonio", conteudo = "Que bom que ajudou." });

        resposta.EnsureSuccessStatusCode();
        var filho = await resposta.Content.ReadFromJsonAsync<ComentarioResponse>();
        Assert.Equal(raiz.Id, filho!.ParentId);

        // 2º nível: o schema aceitaria (parent_id é livre), a regra de negócio não.
        var neto = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { parentId = filho.Id, autor = "Antonio", conteudo = "Resposta da resposta." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, neto.StatusCode);
    }

    [Fact]
    public async Task Responder_comentario_de_outro_post_devolve_422()
    {
        var postA = await CriarPostAsync();
        var postB = await CriarPostAsync();
        var leitor = ClienteVisitante();
        var noPostA = await ComentarAsync(leitor, postA.Id, "Sou do post A");

        var resp = await leitor.PostAsJsonAsync($"/api/v1/posts/{postB.Id}/comentarios",
            new { parentId = noPostA.Id, autor = "Intruso", conteudo = "Conversa trocada." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Comentar_em_post_inexistente_devolve_404()
    {
        var resp = await ClienteVisitante().PostAsJsonAsync(
            $"/api/v1/posts/{Guid.NewGuid()}/comentarios",
            new { autor = "Ninguém", conteudo = "Post fantasma." });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Palavrao_e_bloqueado_com_422_e_nao_entra_no_banco()
    {
        var post = await CriarPostAsync();

        var resp = await ClienteVisitante().PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Anônimo", conteudo = "que MERDA de texto" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        Assert.False(await db.Comentarios.AnyAsync(c => c.PostId == post.Id));
    }

    [Fact]
    public async Task Sexto_comentario_no_mesmo_minuto_devolve_429()
    {
        var post = await CriarPostAsync();
        var leitor = ClienteVisitante();

        for (var i = 1; i <= 5; i++)
            await ComentarAsync(leitor, post.Id, $"Comentário {i}");

        var sexto = await leitor.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Afobado", conteudo = "Mais um!" });

        Assert.Equal(HttpStatusCode.TooManyRequests, sexto.StatusCode);

        // Outro visitante (User-Agent diferente → outro hash) não é afetado.
        var outro = await ClienteVisitante().PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Calmo", conteudo = "Cheguei agora." });

        outro.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Lista_arvore_rasa_com_respostas_e_envelope_paginado()
    {
        var post = await CriarPostAsync();
        var raiz = await ComentarAsync(ClienteVisitante(), post.Id, "Primeira raiz");
        await ResponderAsync(ClienteVisitante(), post.Id, raiz.Id, "Resposta da raiz");
        await ComentarAsync(ClienteVisitante(), post.Id, "Segunda raiz");

        // Anônimo: listar não pede token.
        var pagina = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");

        // TotalItems conta RAÍZES (2), não as 3 linhas da tabela.
        Assert.Equal(2, pagina!.TotalItems);
        Assert.Equal(2, pagina.Items.Count);

        var primeira = pagina.Items[0];
        Assert.Equal("Primeira raiz", primeira.Conteudo);
        Assert.Equal("Resposta da raiz", Assert.Single(primeira.Respostas).Conteudo);
        Assert.Empty(pagina.Items[1].Respostas);
        Assert.NotEqual(default, primeira.DataCriacao);   // agora sim: veio do banco
    }

    [Fact]
    public async Task Filtro_e_paginacao_da_querystring_valem_mas_aprovado_nao_e_filtravel()
    {
        var post = await CriarPostAsync();
        await ComentarAsync(ClienteVisitante(), post.Id, "Sobre respirar");
        var oculto = await ComentarAsync(ClienteVisitante(), post.Id, "Vai sumir");

        var escondeu = await (await ClienteAdminAsync()).PatchAsync(
            $"/api/v1/posts/{post.Id}/comentarios/{oculto.Id}/ocultar", new StringContent(string.Empty));

        Assert.Equal(HttpStatusCode.NoContent, escondeu.StatusCode);

        var anonimo = factory.CreateClient();

        // Escondido não volta nem pedindo explicitamente: `aprovado` não está no mapper,
        // e campo não mapeado é IGNORADO (IgnoreNotMappedFields no Program).
        var tentativa = await anonimo.GetFromJsonAsync<Pagina<ComentarioListaResponse>>(
            $"/api/v1/posts/{post.Id}/comentarios?filter=aprovado=false");

        Assert.Equal(1, tentativa!.TotalItems);
        Assert.DoesNotContain(tentativa.Items, c => c.Id == oculto.Id);

        // O que ESTÁ no mapper funciona normalmente.
        var porAutor = await anonimo.GetFromJsonAsync<Pagina<ComentarioListaResponse>>(
            $"/api/v1/posts/{post.Id}/comentarios?filter=autor=Ninguém&pageSize=1");

        Assert.Equal(0, porAutor!.TotalItems);
    }

    [Fact]
    public async Task Admin_ve_o_oculto_na_listagem_e_consegue_reexibir()
    {
        var post = await CriarPostAsync();
        var raiz = await ComentarAsync(ClienteVisitante(), post.Id, "Some e volta");
        var admin = await ClienteAdminAsync();

        await admin.PatchAsync(
            $"/api/v1/posts/{post.Id}/comentarios/{raiz.Id}/ocultar", new StringContent(string.Empty));

        // Anônimo não vê; admin vê, marcado como oculto — é o que a tela de moderação usa.
        var paraAnonimo = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");
        Assert.DoesNotContain(paraAnonimo!.Items, c => c.Id == raiz.Id);

        var paraAdmin = await admin
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");
        Assert.False(Assert.Single(paraAdmin!.Items, c => c.Id == raiz.Id).Aprovado);

        var reexibiu = await admin.PatchAsync(
            $"/api/v1/posts/{post.Id}/comentarios/{raiz.Id}/reexibir", new StringContent(string.Empty));
        Assert.Equal(HttpStatusCode.NoContent, reexibiu.StatusCode);

        var voltou = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");
        Assert.True(Assert.Single(voltou!.Items, c => c.Id == raiz.Id).Aprovado);
    }

    [Fact]
    public async Task Admin_deleta_comentario_e_a_resposta_cai_junto()
    {
        var post = await CriarPostAsync();
        var raiz = await ComentarAsync(ClienteVisitante(), post.Id, "Raiz condenada");
        var resposta = await ResponderAsync(ClienteVisitante(), post.Id, raiz.Id, "Vou junto");

        var apagou = await (await ClienteAdminAsync())
            .DeleteAsync($"/api/v1/posts/{post.Id}/comentarios/{raiz.Id}");

        Assert.Equal(HttpStatusCode.NoContent, apagou.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();

        // Cascata da auto-referência parent_id: quem apaga a resposta é o banco.
        Assert.False(await db.Comentarios.AnyAsync(c => c.Id == raiz.Id || c.Id == resposta.Id));
    }

    [Fact]
    public async Task Sem_token_401_e_com_token_de_nao_admin_403()
    {
        var post = await CriarPostAsync();
        var alvo = await ComentarAsync(ClienteVisitante(), post.Id, "Comentário inocente");
        var rota = $"/api/v1/posts/{post.Id}/comentarios/{alvo.Id}";

        var anonimo = await factory.CreateClient().DeleteAsync(rota);
        Assert.Equal(HttpStatusCode.Unauthorized, anonimo.StatusCode);

        // Autenticado mas sem a claim: 403 (sei quem é você, não pode) — não 401.
        var comum = await (await ClienteAsync(admin: false)).DeleteAsync(rota);
        Assert.Equal(HttpStatusCode.Forbidden, comum.StatusCode);
    }

    [Fact]
    public async Task Logado_assina_com_o_nome_da_conta_e_a_lista_marca_o_autor_do_post()
    {
        var (post, autor) = await CriarPostComAutorAsync();
        var dono = ClienteDe(autor);

        // Sem `autor` no corpo de propósito: quem está logado assina com a conta.
        var resp = await dono.PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { conteudo = "Obrigado por escrever, Marina." });

        resp.EnsureSuccessStatusCode();
        var criado = await resp.Content.ReadFromJsonAsync<ComentarioResponse>();
        Assert.Equal(autor.Nome, criado!.Autor);

        var pagina = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");

        var comentario = Assert.Single(pagina!.Items, c => c.Id == criado.Id);
        Assert.True(comentario.EhAutorDoPost);
        Assert.Equal(autor.ImagemUrl, comentario.AutorImagemUrl);
    }

    [Fact]
    public async Task Nome_mandado_no_corpo_nao_sobrescreve_a_assinatura_de_quem_esta_logado()
    {
        var (post, autor) = await CriarPostComAutorAsync();

        var resp = await ClienteDe(autor).PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { autor = "Outra Pessoa", conteudo = "Assinatura vem da conta." });

        resp.EnsureSuccessStatusCode();
        var criado = await resp.Content.ReadFromJsonAsync<ComentarioResponse>();

        Assert.Equal(autor.Nome, criado!.Autor);
    }

    [Fact]
    public async Task Anonimo_e_logado_que_nao_escreveu_o_post_nao_ganham_marca_de_autor()
    {
        var (post, _) = await CriarPostComAutorAsync();
        var anonimo = await ComentarAsync(ClienteVisitante(), post.Id, "Sou leitor de passagem");

        var outro = await (await ClienteAsync(admin: false))
            .PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
                new { conteudo = "Logado, mas não escrevi este post." });
        outro.EnsureSuccessStatusCode();
        var doOutro = await outro.Content.ReadFromJsonAsync<ComentarioResponse>();

        var pagina = await factory.CreateClient()
            .GetFromJsonAsync<Pagina<ComentarioListaResponse>>($"/api/v1/posts/{post.Id}/comentarios");

        var doVisitante = Assert.Single(pagina!.Items, c => c.Id == anonimo.Id);
        Assert.False(doVisitante.EhAutorDoPost);
        Assert.Null(doVisitante.AutorImagemUrl);   // visitante não tem conta, nem foto

        var logado = Assert.Single(pagina.Items, c => c.Id == doOutro!.Id);
        Assert.False(logado.EhAutorDoPost);        // logado sim, dono do post não
        Assert.NotNull(logado.AutorImagemUrl);
    }

    [Fact]
    public async Task Comentario_sem_nome_e_sem_login_devolve_422()
    {
        var post = await CriarPostAsync();

        var resp = await ClienteVisitante().PostAsJsonAsync($"/api/v1/posts/{post.Id}/comentarios",
            new { conteudo = "Anônimo de verdade." });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    private static Task<ComentarioResponse> ResponderAsync(
        HttpClient client, Guid postId, Guid paiId, string texto) =>
        ComentarAsync(client, postId, texto, paiId);

    /// <summary>Cliente com JWT de um usuário do seed (todos admin).</summary>
    private Task<HttpClient> ClienteAdminAsync() => ClienteAsync(admin: true);

    private async Task<HttpClient> ClienteAsync(bool admin)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        // O seed só tem admins — o usuário comum é criado aqui, uma vez.
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.IsAdmin == admin);
        if (usuario is null)
        {
            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Leitor Comum",
                Email = $"comum-{Guid.NewGuid():N}@teste.com",
                GoogleId = Guid.NewGuid().ToString(),
                ImagemUrl = "https://exemplo.com/foto.png",
                IsAdmin = false,
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(usuario));
        return client;
    }

    /// <summary>Cliente com o JWT de um usuário específico — para dizer "este é o dono do post".</summary>
    private HttpClient ClienteDe(Usuario usuario)
    {
        using var scope = factory.Services.CreateScope();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(usuario));
        return client;
    }

    /// <summary>Cliente anônimo com identidade de visitante única (isola o rate limit).</summary>
    private HttpClient ClienteVisitante()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"teste-{Guid.NewGuid():N}/1.0");
        return client;
    }

    private static async Task<ComentarioResponse> ComentarAsync(
        HttpClient client, Guid postId, string texto, Guid? paiId = null)
    {
        var resp = await client.PostAsJsonAsync($"/api/v1/posts/{postId}/comentarios",
            new { parentId = paiId, autor = "Leitor", conteudo = texto });

        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ComentarioResponse>())!;
    }

    private async Task<PostResponse> CriarPostAsync() => (await CriarPostComAutorAsync()).Post;

    private async Task<(PostResponse Post, Usuario Autor)> CriarPostComAutorAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PensaComigoDbContext>();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        // Admin de propósito: o "Leitor Comum" é o contraponto nos testes de marca de autor.
        var autor = await db.Usuarios.FirstAsync(u => u.IsAdmin);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Gerar(autor));

        var resp = await client.PostAsJsonAsync("/api/v1/posts", new
        {
            titulo = $"Post pra comentar {Guid.NewGuid():N}",
            imagemCapa = "posts/capa.webp",
            tagIds = Array.Empty<Guid>(),
            conteudo = new object[]
            {
                new { tipo = TipoBloco.Texto, ordem = 1, html = "<p>respirar fundo</p>" },
            },
        });

        resp.EnsureSuccessStatusCode();
        return ((await resp.Content.ReadFromJsonAsync<PostResponse>())!, autor);
    }
}
