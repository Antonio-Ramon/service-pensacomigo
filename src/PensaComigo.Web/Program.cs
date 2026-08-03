using System.Net.Http.Headers;
using System.Text;
using Gridify;
using Gridify.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PensaComigo.Application;
using PensaComigo.Application.Auth;
using PensaComigo.Application.Storage;
using PensaComigo.Persistence;
using PensaComigo.Web.Auth;
using PensaComigo.Web.Exceptions;
using PensaComigo.Web.Storage;
using PensaComigo.Web.Swagger;

// Gridify (padrão de listagem, arquitetura §7.1): traduz o filtro pra SQL via EF Core e
// ignora campo não mapeado no GridifyMapper em vez de estourar exceção.
GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
GridifyGlobalConfiguration.IgnoreNotMappedFields = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
    });
builder.Services.AddAuthorization();

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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Já abre em modo editável: sem precisar clicar em "Try it out" a cada endpoint.
    app.UseSwaggerUI(ui => ui.EnableTryItOutByDefault());
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Torna Program referenciável pelos testes (WebApplicationFactory<Program>).
// Top-level statements geram um Program internal; este partial o expõe.
public partial class Program;
