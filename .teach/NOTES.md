# Notas

## Preferências
- Vem de outra linguagem — pode pular explicação de conceitos universais (loop, if),
  focar no que é **específico de C#/.NET** e das arquiteturas.
- Concisão > gramática na comunicação.
- Quer prática ativa: implementar junto, não só ler código pronto.

## Progresso
- [x] Fatia 1 — Mapeamento EF Core + DbContext (aula 0001) — build verde
- [x] Fatia 2 — jsonb via HasConversion + ValueComparer (aula 0002). Código já
  estava no PostConfiguration desde a Fatia 1; esta fatia foi entendimento.
- [x] Fatia 3 — Repository pattern + DI (aula 0003). Interfaces no Domain, 4 impls
  esqueleto no Persistence, `AddPersistence()` no Program.cs. Build verde.
  DbContext ainda fora da DI (vem na Fatia 4).
- [x] Fatia 4 — Migration inicial + seed + AddDbContext (aula 0004). DbContext na DI,
  seed HasData (Antonio/Jéssica), migration `InicialSchema` gerada e conferida.
  Falta só `database update` contra Supabase real (passo de infra do usuário).
- [x] Fatia 5 — Pipeline MediatR (aula 0005). 3 behaviors (Logging/Validation/UnitOfWork),
  marcadores CQRS (`ICommand`/`IQuery`/`IBaseCommand`), ponte `IUnitOfWork` (DbContext).
  `AddApplication()` no Program. Build verde. Sem Command/validator real ainda — é o trilho.
- [x] Fatia 7 — Swagger + JWT + versionamento (aula 0007). `AddAuthentication/AddJwtBearer`
  valida JWT PRÓPRIO (chave simétrica, `Jwt:Key` via user-secrets), todos `Validate*` on.
  `UseAuthentication` antes de `UseAuthorization`. Swagger com botão Authorize (Bearer).
  **Versionamento = convenção de rota** `[Route("api/v1/[controller]")]`; lib `Asp.Versioning`
  adiada até existir v2. Build + boot verdes. Swashbuckle 10 usa OpenApi 2.x (churn de API
  resolvido: ns `Microsoft.OpenApi`, `OpenApiSecuritySchemeReference`, AddSecurityRequirement=factory).
- [x] Fatia 8 — Harness de integração (aula 0008). `public partial class Program;` expõe o tipo;
  `WebApplicationFactory<Program>` + `Testcontainers.PostgreSql` sobem app+Postgres real; migration
  aplicada no `IAsyncLifetime.InitializeAsync`; override só de `ConnectionStrings:Default` (DI intacta).
  Smoke test (seed Antonio/Jessica). Build verde. **Docker não está nesta máquina** → teste não rodou
  aqui (passo de infra). Choque de `DisposeAsync` (xunit Task vs WAF ValueTask) → interface explícita.
  **Ticket 01 fechado no código.**
- [x] Fatia 6 — ExceptionHandling + exceções tipadas (aula 0006). `NaoEncontradoException`(404)
  e `RegraDeNegocioException`(422) no Domain; `GlobalExceptionHandler : IExceptionHandler`
  (nativo .NET, NÃO middleware manual) casa tipo→status via switch, emite ProblemDetails RFC 7807.
  `ValidationException`(FluentValidation)→422 com erros campo-a-campo. `AddProblemDetails` +
  `AddExceptionHandler` + `app.UseExceptionHandler()`. Build verde. **429 ficou fora** (sem
  rate limiter ainda). Sem controller p/ exercitar de ponta a ponta — vem na Fatia 8 (harness).

- [x] Fatia 9 — Caso de uso vertical CQRS (aula 0009). Primeiro Command/Handler/Controller reais:
  `LoginGoogleCommand`, handler com a regra, `AuthController` magro (Send), primeiro validator
  (achado por AddValidatorsFromAssembly). Google/JWT como seams (interfaces) — impl na Fatia 10.
  `ObterPorEmailAsync` no repo. Build verde. **Endpoint não roda ponta a ponta ainda** (faltam impls+DI).
  Abriu o Ticket 02.

- [x] Fatia 10 — Seams Google + JWT (aula 0010). `GoogleTokenValidator` (Google.Apis.Auth 1.75.0,
  valida assinatura + `aud==Google:ClientId`) e `JwtTokenGenerator` (chave simétrica `Jwt:Key`,
  claims sub/email/is_admin, 8h) em `Web/Auth/`. 2 `AddScoped` no Program ligam os seams. `Google:ClientId`
  no appsettings. Build verde (8 proj, 0 warn). Impls no HOST (sem projeto Infrastructure — YAGNI).
  Token Google inválido → 422 (reusa RegraDeNegocio; sem tipo p/ 401 ainda, `ponytail:` no validator).
  **Pendências infra**: ClientId/Jwt:Key reais via secrets; teste integração do login precisa de Docker.

## Cuidado ao montar quiz
- `data-a` é índice 0-based do botão correto. Já saiu errado 2x na aula 05 (embaralhei a
  posição da resposta mas não atualizei o índice). SEMPRE reconferir: contar os botões de 0 e
  bater com o `data-a` antes de entregar.

## Observações técnicas encontradas
- `Usuario.IsAdmin` na entidade tem default `true`, mas schema/ticket pede coluna
  default `false`. Corrigido no default da COLUNA (config). Vale conferir o default
  da propriedade C# depois (seed cria admins explicitamente).
