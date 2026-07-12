# 01 — Fundação: Persistence + Spine Web/Application

**What to build:** A base compartilhada que faz a API subir, conectar no Postgres do Supabase e aceitar testes de integração ponta a ponta. Ninguém vê caso de uso ainda, mas a partir daqui toda fatia de negócio (02+) é fina e verde.

**Blocked by:** None — can start immediately.

**Status:** in-progress

Persistence:
- [x] `PensaComigoDbContext` com `EntityTypeConfiguration` de todas as entidades conforme schema §5.4
- [x] Contadores desnormalizados em `posts`; `comentarios.parent_id` auto-referência; `likes` unique `(post_id, viewer_hash)`; `usuarios.is_admin` default `false`; **sem** índice GIN
- [x] `Post.Conteudo` (`List<Bloco>`) persiste em coluna `jsonb` via conversor manual (`HasConversion` + `JsonSerializer`), tratado como blob
- [x] Interfaces (`IPostRepository`, `IComentarioRepository`, `IUsuarioRepository`, `ITagRepository`) no Domain + implementações esqueleto no Persistence (métodos crescem por fatia)
- [x] Migration inicial + seed `UsuariosSeed` (Antonio Ramon, Jessica Rose) — aplicada no Supabase (`database update` verde)
- [x] Conexão Supabase: `AddDbContext` + connection string `Default` via user-secrets. Porta 5432 direta p/ migration e runtime (tráfego baixo); pooler 6543 só se escalar. `SSL Mode=Require`

Spine Web/Application:
- [ ] Behaviors MediatR: `ValidationBehavior`, `LoggingBehavior`, `UnitOfWorkBehavior` (commit atômico só em Commands)
- [ ] `ExceptionHandlingMiddleware` mapeando exceções tipadas → 404 / 422 / 429; erros de FluentValidation no mesmo pipe; controllers magros
- [ ] Swagger/OpenAPI + versionamento `/api/v1`
- [ ] Config JwtBearer (validação de token nas rotas de escrita)
- [ ] `public partial class Program {}` no fim do `Program.cs`
- [ ] Harness de integração: `WebApplicationFactory<Program>` + Testcontainers (Postgres real)

**Verificável:** migration aplica num Postgres via Testcontainers; API sobe; Swagger abre; teste de integração vazio roda verde.
