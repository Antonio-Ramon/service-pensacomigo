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

- [x] Fatia 11 — Endpoint protegido + claims (aula 0011). `[Authorize] GET usuarios/me` (401 auto),
  lê `sub` do `User` (`ClaimsPrincipal`), `MapInboundClaims=false` no Program p/ nome da claim intacto.
  Primeiro **Query** CQRS (`ObterPerfilQuery : IQuery<>`, sem commit). Build verde (8 proj, 0 erro).
  Warning MSB3277 (conflito EF 10.0.4/10.0.9 em IntegrationTests) pré-existente. Não roda ponta a
  ponta aqui (precisa JWT real + Docker). **Ticket 02 fechado no código.**

- [x] Fatia 10 — Seams Google + JWT (aula 0010). `GoogleTokenValidator` (Google.Apis.Auth 1.75.0,
  valida assinatura + `aud==Google:ClientId`) e `JwtTokenGenerator` (chave simétrica `Jwt:Key`,
  claims sub/email/is_admin, 8h) em `Web/Auth/`. 2 `AddScoped` no Program ligam os seams. `Google:ClientId`
  no appsettings. Build verde (8 proj, 0 warn). Impls no HOST (sem projeto Infrastructure — YAGNI).
  Token Google inválido → 422 (reusa RegraDeNegocio; sem tipo p/ 401 ainda, `ponytail:` no validator).
  **Pendências infra**: ClientId/Jwt:Key reais via secrets; teste integração do login precisa de Docker.

- [x] Fatia 12 — Feature Tags: Command + Query juntos (aula 0012). Primeira feature CRUD não-auth.
  `CriarTagCommand`([Authorize]) + `ListarTagsQuery`([AllowAnonymous]) no MESMO `TagsController`.
  Slug calculado no handler (Normalize FormD → tira NonSpacingMark → regex hífen); `RegraDeNegocioException`
  (422) na colisão de slug antes do índice único (evita 500). Repo ganhou `ListarAsync`(AsNoTracking/OrderBy)
  + `ExistePorSlugAsync`. Teste integração `TagsTests` (401 sem token / 200+slug com token do seed via
  IJwtTokenGenerator da DI / GET anônimo lista). Build verde (8 proj, 0 erro). **Sem Docker aqui** → teste
  não rodou (infra do usuário). Warning MSB3277 EF pré-existente. **Ticket 03 fechado no código.**

- [x] Fatia 13 — Gridify, padrão de listagem project-wide (aula 0013). NuGet `Gridify` 2.19.1 +
  `Gridify.EntityFramework` (vendorar do escolaweb descartado). `Pagina<T>` em `Domain/Common`
  (nome pt-br: o pacote já tem `Paging<T>` → `CS0104`). `ListarTagsQuery : GridifyQuery` virou
  **class** (record não herda de classe comum), controller com `[FromQuery]`. `TagRepository`
  usa `GridifyQueryableAsync` + `GridifyMapper` whitelist (nome/slug) e força `OrderBy=nome`
  (sem ORDER BY a paginação é instável). Config global no `Program.cs`. `TagsTests` cobre envelope
  + `?filter=`. Build verde (8 proj, 0 erro). **Sem Docker aqui** → teste não rodou.
  Fecha follow-up da issue 01 e conforma a issue 03. Aula 12 atualizada com nota + link.

- [x] Fatia 14 — Imagens signed URL: typed HttpClient + Options (aula 0014). Seam `IStorage`
  (Application) + `SupabaseStorage` (Web) via `AddHttpClient<IStorage, SupabaseStorage>` — o typed
  client JÁ registra a impl. `SupabaseOptions` com `ValidateDataAnnotations().ValidateOnStart()`
  (app não sobe sem ServiceRoleKey → factory de teste ganhou 2 `UseSetting`). Caso de uso é
  **`IQuery`** (não escreve no banco). **Path montado no servidor** (`posts/{claim sub}/{guid}{ext}`),
  cliente só manda o nome — desvio consciente da issue, mata path traversal. `ImagensTests` troca o
  seam por fake via `ConfigureTestServices`. Build verde (8 proj, 0 erro). **Sem Docker aqui** → teste
  não rodou. Quiz agora usa `assets/quiz.js` (extraído; aulas antigas seguem com script inline).

- [x] Fatia 15 — Função pura + 1º teste unitário (aula 0015). `GeradorSlug` (`Gerar` +
  `ResolverColisao(base, ocupados)`) e `CalculadoraTempoLeitura` em `Application/Common` —
  **não no `Shared`**: a seta é `Shared → Application`, então Shared seria invisível pro handler.
  `CriarTagCommandHandler` deixou de duplicar a normalização. 11 métodos → **18 testes, 358 ms,
  todos verdes** (primeiro teste que ROda nesta máquina — não precisa de Docker). Build verde
  (8 proj, 0 erro). Fecha 2 dos 7 itens da issue 05.

- [x] Fatia 16 — `CriarPostCommand`: N:N + jsonb (aula 0016). `CriarPostRequest` (corpo) separado do
  Command (`AutorId` da claim). Handler = impureim sandwich: `ListarSlugsComPrefixoAsync` (LIKE
  prefixo%) → `ResolverColisao` puro → `ObterPorIdsAsync` (**único método de repo SEM AsNoTracking**:
  entidade rastreada faz o EF inserir só `post_tags`, sem reinserir a tag) → `Calcular` tempo →
  `AdicionarAsync`. Validator com `RuleForEach` + switch expression cobrindo o modelo flat do Bloco.
  Tag inexistente → 404 via `Except()`. `PostsTests` (401 / jsonb+junção+colisão `-2` / 422).
  Build verde (8 proj, 0 erro), 18 unit tests verdes. **Sem Docker aqui** → integração não rodou.
  Fecha 4 dos 7 itens da issue 05.

- [x] Fatia 17 — Upload multipart pelo backend (aula 0017). **Decisão #14 revisada**: signed URL saiu,
  `POST /api/v1/imagens` recebe `IFormFile` e repassa ao Supabase. `IFormFile` para no controller →
  Command leva nome/tamanho/`Stream` (não `byte[]`: LOH). `ImagensPermitidas` = whitelist única
  (extensão→content-type + 5 MB); **content-type sai dela, não do cliente** (XSS). `IQuery`→`ICommand`
  (critério agora é efeito colateral, não escrita no Postgres). `GerarUrlUploadQuery` deletado.
  `ImagensTests` reescrito com `MultipartFormDataContent`; fake agora REGISTRA o content-type recebido.
  Build verde (8 proj, 0 erro), 18 unit tests verdes. Docs (arquitetura + spec + issue 04) atualizados.

- [x] Fatia 18 — Editar + deletar post (aula 0018). `EditarPostCommand`/`DeletarPostCommand`,
  `PUT`/`DELETE {id:guid}`, `[Authorize]` subiu para o controller inteiro. Edição carrega
  RASTREADO com `Include(Tags)` (`ObterParaEdicaoAsync`) — sem Include as tags antigas nunca
  saem da junção; `post.Tags = vinculadas` gera o delta. Slug congelado **por omissão** (sem
  campo no request, sem escrita no handler). Não-dono → **404** (403 vazaria existência),
  checado no handler, não no atributo. Regras de validação extraídas p/ `PostEscritaValidator<T>`
  genérico sobre `IPostEscrita` (record implementa a interface de graça; scanner só acha as
  subclasses fechadas). Delete físico + cascata do schema; `ICommand<Unit>` → 204. Build verde
  (8 proj, 0 erro), 18 unit tests verdes. 3 testes de integração novos **sem Docker aqui**.
  **Ticket 05 fechado.**

- [x] Fatia 19 — Feed público: listar + abrir + visualizações (aula 0019). `ListarPostsQuery : GridifyQuery`
  ([AllowAnonymous]) e `AbrirPostCommand(slug)` no `PostsController` que é `[Authorize]` na classe —
  **AllowAnonymous na ação vence**, o contrário não. `{slug}` não conflita com `{id:guid}` (verbo +
  constraint). Abrir é **`ICommand`** apesar do GET (critério = efeito colateral, Fatia 17); contador
  sobe com `ExecuteUpdateAsync(SetProperty(p => p.Qtd, p => p.Qtd + 1))` → `coluna = coluna + 1`
  atômico, sem lost update; não passa pelo change tracker, daí o handler devolver `+ 1`.
  `PostResumoResponse` (sem jsonb) ≠ `PostDetalheResponse` (conteúdo + autor + tags, `Include` +
  `AsNoTracking`). Mapper com coleção: `.AddMap("tag", p => p.Tags.Select(t => t.Slug))` → EXISTS na
  junção; `OrderBy` default `dataCriacao desc`. Build verde (8 proj, 0 erro), 18 unit tests verdes.
  4 testes de integração novos **sem Docker aqui**. **Ticket 06 fechado.**

- [x] Fatia 20 — Comentários: escrita + moderação automática (aula 0020). `CriarComentarioCommand`
  anônimo em rota aninhada `posts/{postId:guid}/comentarios`. **Primeiro estado fora do banco**:
  `IMemoryCache` + `LimitadorDeComentarios` como **Singleton** (Scoped nasceria vazio; singleton
  nunca injeta scoped — *captive dependency*). Funções puras `FiltroPalavrao` (reusa
  `GeradorSlug.Gerar`, compara palavra inteira) e `JanelaDeslizante.Registrar` (**relógio como
  parâmetro** → testa 1 min sem esperar 1 min; `null` = estourou). Palavrão no **validator**
  (só vê o command), "resposta de resposta" no **handler** (precisa ler o pai). `viewer_hash`
  calculado no servidor (SHA-256 IP+UA). `MuitasRequisicoesException` → **429** (fecha o buraco
  da Fatia 6). `ComentarioResponse` sem `DataCriacao` (commit é depois do handler). Build verde,
  **33 unit tests verdes** (18+15). 6 testes de integração **sem Docker aqui**. Issue 07: 5/7.

## Cuidado ao montar quiz
- `data-a` é índice 0-based do botão correto. Já saiu errado 2x na aula 05 (embaralhei a
  posição da resposta mas não atualizei o índice). SEMPRE reconferir: contar os botões de 0 e
  bater com o `data-a` antes de entregar.

## Observações técnicas encontradas
- `Usuario.IsAdmin` na entidade tem default `true`, mas schema/ticket pede coluna
  default `false`. Corrigido no default da COLUNA (config). Vale conferir o default
  da propriedade C# depois (seed cria admins explicitamente).
