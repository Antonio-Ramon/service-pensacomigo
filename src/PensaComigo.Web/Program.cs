using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PensaComigo.Application;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Storage;
using PensaComigo.Persistence;
using PensaComigo.Shared.Erros;
using PensaComigo.Web.Auth;
using PensaComigo.Web.Exceptions;
using PensaComigo.Web.Storage;
using PensaComigo.Web.Swagger;
using PensaComigo.Web.Visitantes;

// Gridify (padrão de listagem, arquitetura §7.1): traduz o filtro pra SQL via EF Core e
// ignora campo não mapeado no GridifyMapper em vez de estourar exceção.
GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
GridifyGlobalConfiguration.IgnoreNotMappedFields = true;

var builder = WebApplication.CreateBuilder(args);

// O MVC transforma propriedade não-anulável em [Required] implícito e rejeita o corpo ANTES do
// pipeline — com mensagem em inglês e num formato só dele. Desligado: quem valida corpo é o
// FluentValidation, num lugar só, em pt-br (ValidationBehavior → 422).
builder.Services.AddControllers(mvc =>
{
    mvc.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.ConfigureApiBehaviorOptions(mvc =>
{
    // Sobra o que o binder não consegue nem ler: JSON quebrado, corpo vazio, guid inválido
    // na rota. Continua 400 (não entendi o pedido) — mas no MESMO envelope de erro do
    // GlobalExceptionHandler, senão o front trataria dois formatos.
    mvc.InvalidModelStateResponseFactory = ctx =>
    {
        var resposta = new RespostaErro
        {
            Message = "Não foi possível ler a requisição. Confira o formato dos campos.",
            Notifications =
            [
                .. ctx.ModelState
                     .Where(campo => campo.Value?.Errors.Count > 0)
                     .SelectMany(campo => campo.Value!.Errors
                         .Select(e => new Notificacao(campo.Key, e.ErrorMessage))),
            ],
        };

        return new ObjectResult(resposta) { StatusCode = StatusCodes.Status400BadRequest };
    };
});
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

// Impls dos seams de auth (Fatia 10). Ficam no host: dependem de config e de libs externas
// que a Application não pode conhecer. O teste de integração troca IGoogleTokenValidator por fake.
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Storage (Fatia 14). Options pattern: seção tipada + validada NA SUBIDA — sem ServiceRoleKey
// a aplicação nem sobe, em vez de falhar no primeiro upload.
builder.Services.AddOptions<SupabaseOptions>()
    .Bind(builder.Configuration.GetSection("Supabase"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Typed client: registra IStorage E o HttpClient dele. O handler HTTP fica num pool e é
// reciclado — nem esgota socket (new HttpClient por chamada) nem envelhece o DNS (static).
// Barra final na BaseAddress + caminho relativo sem barra inicial, senão o /storage/v1 some.
builder.Services.AddHttpClient<IStorage, SupabaseStorage>((sp, http) =>
{
    var supabase = sp.GetRequiredService<IOptions<SupabaseOptions>>().Value;
    http.BaseAddress = new Uri($"{supabase.Url.TrimEnd('/')}/storage/v1/");
    http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", supabase.ServiceRoleKey);
});

// Atrás de proxy/ingress o RemoteIpAddress é o do PROXY para todo mundo, e a identidade do
// visitante (IP + User-Agent) colapsa: dois leitores com o mesmo navegador viram um só.
var proxiesConfiaveis = builder.Configuration.GetSection("ProxiesConfiaveis").Get<string[]>() ?? [];
builder.Services.Configure<ForwardedHeadersOptions>(opcoes =>
{
    // CUIDADO: o middleware só checa a origem se HOUVER allowlist — com as duas listas vazias
    // ele aceita X-Forwarded-For de qualquer cliente, que aí escolhe a própria identidade
    // (curte o mesmo post infinitas vezes, zera o rate limit dos comentários). Por isso sem
    // config o header é simplesmente ignorado, em vez de "confiar em todo mundo".
    if (proxiesConfiaveis.Length == 0)
    {
        opcoes.ForwardedHeaders = ForwardedHeaders.None;
        return;
    }

    opcoes.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // O default confia no loopback; quem chega em produção é o ingress, então a lista vem do config.
    opcoes.KnownProxies.Clear();
    opcoes.KnownIPNetworks.Clear();

    foreach (var proxy in proxiesConfiaveis)
    {
        // IP solto ou CIDR — ingress em k8s/cloud LB é faixa, não endereço fixo.
        if (proxy.Contains('/'))
        {
            opcoes.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(proxy));
        }
        else
        {
            opcoes.KnownProxies.Add(IPAddress.Parse(proxy));
        }
    }
});

// Pepper do hash do visitante. Mesmo padrão do Supabase: validado NA SUBIDA — sem ele a
// aplicação não sobe, em vez de estourar 500 na primeira curtida.
builder.Services.AddOptions<VisitantesOptions>()
    .Bind(builder.Configuration.GetSection("Visitantes"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<HashVisitante>();

// Curtidas e comentarios sao chamados PELO BROWSER, nao pelo servidor do front: o viewer_hash
// nasce do IP da conexao, e um proxy no front colapsaria todos os leitores num visitante so
// (rate limit de comentario global, curtida de terceiro virando no-op). Dai o CORS.
var origensFront = builder.Configuration.GetSection("OrigensFront").Get<string[]>() ?? [];
builder.Services.AddCors(opcoes => opcoes.AddDefaultPolicy(p => p
    .WithOrigins(origensFront)
    .WithMethods("GET", "POST", "DELETE")
    .WithHeaders("Content-Type")));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Valida o JWT PRÓPRIO (emitido no login da Fatia 02), chave simétrica via user-secrets.
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sem isso o handler renomeia `sub`→`nameidentifier`, `email`→`.../emailaddress` etc.
        // Desligado: as claims chegam no User com o MESMO nome que o JwtTokenGenerator escreveu.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? "")),
        };

        // Token ausente/inválido é rejeitado pelo middleware, longe do GlobalExceptionHandler —
        // e o padrão é 401 com corpo VAZIO. Aqui ele passa a falar o mesmo envelope dos demais.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();   // cancela a resposta padrão do handler
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await contexto.Response.WriteAsJsonAsync(new RespostaErro
                {
                    Message = "Autenticação necessária. Envie um token válido.",
                    Notifications = [new Notificacao("Erro", "Token ausente, inválido ou expirado.")],
                });
            },
        };
    });
// Primeira autorização por CLAIM, não por "tem token": [Authorize] só exige autenticado;
// a policy exige `is_admin=true` no JWT. Quem não é admin toma 403 (autenticado, sem permissão),
// não 401. Regra num lugar só — mudar aqui muda todos os [Authorize(Policy = "Admin")].
builder.Services.AddAuthorization(opcoes =>
    opcoes.AddPolicy("Admin", politica => politica.RequireClaim("is_admin", "true")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Botão "Authorize": manda o Bearer token nas rotas [Authorize].
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    // Cadeado por rota (só nas [Authorize]) em vez de requisito global.
    options.DocumentFilter<SecurityRequirementOperationFilter>();
});

var app = builder.Build();

// Antes de tudo que lê IP ou esquema: reescreve RemoteIpAddress a partir do X-Forwarded-For.
app.UseForwardedHeaders();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Já abre em modo editável: sem precisar clicar em "Try it out" a cada endpoint.
    app.UseSwaggerUI(ui => ui.EnableTryItOutByDefault());
}

// Em dev o front (Node) rejeita o dev-cert self-signed: sem redirect, http://localhost:5001 serve direto.
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
// wwwroot/login.html: página de login servida pela própria API → mesma origem, sem CORS.
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Torna Program referenciável pelos testes (WebApplicationFactory<Program>).
// Top-level statements geram um Program internal; este partial o expõe.
public partial class Program;
