# 01 — Fundação: Persistence + Spine Web/Application

**What to build:** A base compartilhada que faz a API subir, conectar no Postgres do Supabase e aceitar testes de integração ponta a ponta. Ninguém vê caso de uso ainda, mas a partir daqui toda fatia de negócio (02+) é fina e verde.

**Blocked by:** None — can start immediately.

**Status:** done (código completo; smoke test roda verde com Docker no ar)

Persistence:
- [x] `PensaComigoDbContext` com `EntityTypeConfiguration` de todas as entidades conforme schema §5.4
- [x] Contadores desnormalizados em `posts`; `comentarios.parent_id` auto-referência; `likes` unique `(post_id, viewer_hash)`; `usuarios.is_admin` default `false`; **sem** índice GIN
- [x] `Post.Conteudo` (`List<Bloco>`) persiste em coluna `jsonb` via conversor manual (`HasConversion` + `JsonSerializer`), tratado como blob
- [x] Interfaces (`IPostRepository`, `IComentarioRepository`, `IUsuarioRepository`, `ITagRepository`) no Domain + implementações esqueleto no Persistence (métodos crescem por fatia)
- [x] Migration inicial + seed `UsuariosSeed` (Antonio Ramon, Jessica Rose) — aplicada no Supabase (`database update` verde)
- [x] Conexão Supabase: `AddDbContext` + connection string `Default` via user-secrets. Porta 5432 direta p/ migration e runtime (tráfego baixo); pooler 6543 só se escalar. `SSL Mode=Require`

Spine Web/Application:
- [x] Behaviors MediatR: `ValidationBehavior`, `LoggingBehavior`, `UnitOfWorkBehavior` (commit atômico só em Commands)
- [x] `GlobalExceptionHandler` (`IExceptionHandler` nativo) mapeando exceções tipadas → 404 / 422; erros de FluentValidation no mesmo pipe; ProblemDetails RFC 7807. 429 fica p/ quando houver rate limiter
- [x] Swagger/OpenAPI (com botão Authorize/Bearer) + versionamento `/api/v1` via convenção de rota (`[Route("api/v1/[controller]")]`; lib `Asp.Versioning` só quando existir v2)
- [x] Config JwtBearer: valida JWT próprio (chave simétrica, todos `Validate*` on), `UseAuthentication` antes de `UseAuthorization`. `[Authorize]` nas rotas de escrita entra com os controllers
- [x] `public partial class Program {}` no fim do `Program.cs`
- [x] Harness de integração: `WebApplicationFactory<Program>` + Testcontainers (Postgres real). Smoke test verde exige Docker rodando (passo de infra do usuário)

**Verificável:** migration aplica num Postgres via Testcontainers; API sobe; Swagger abre; teste de integração vazio roda verde.
