# 0003 — Fatia 4: Migration inicial + seed + conexão

**Data:** 2026-07-11
**Aula:** lessons/0004-migration-seed-conexao.html

## O que foi coberto
- Migration = diff entre snapshot e modelo atual. `migrations add` é **offline**
  (não toca banco); `database update` é quem aplica no Postgres.
- Design-time: `dotnet ef ... -s Web` sobe o Program.cs pra achar o DbContext na DI.
  Por isso a Fatia 4 finalmente registra `AddDbContext` (encerra o ponytail da Fatia 3).
- `HasData`: seed entra na própria migration (INSERT no Up). Exige PK fixa e valores
  explícitos em colunas obrigatórias (o `now()` default não vale pra linha semeada).
- Conexão Supabase: **uma porta só (5432 direta) p/ migration e runtime** nesta
  escala. Pooler 6543 só sob tráfego alto (e quebra DDL em transaction mode).
  `SSL Mode=Require` sempre. String via `user-secrets` (fora do git).

## Entregue no código
- `DependencyInjection.AddPersistence(IConfiguration)` → `AddDbContext<PensaComigoDbContext>`
  com `UseNpgsql(GetConnectionString("Default"))`. Program.cs passa `builder.Configuration`.
- `UsuarioConfiguration.HasData(Antonio, Jéssica)` — Guids e DataCriacao fixos, IsAdmin true.
- appsettings.json: `ConnectionStrings:Default` vazio (segredo via user-secrets/env).
  appsettings.Development.json: Postgres local.
- Manifesto de tool local (`dotnet-tools.json`) + `dotnet-ef` 10.0.9.
- Migration `20260711202244_InicialSchema` gerada e conferida: sem GIN, is_admin
  default false, parent_id auto-ref, likes unique (post_id, viewer_hash),
  post.conteudo jsonb, seed dos 2 autores. Build verde.

## Decisões / ponytail
- Uma connection string `Default`, porta 5432 direta p/ tudo (decisão do usuário,
  confirmada em sessão anterior). Pooler fica pra quando/se o tráfego crescer.
- `database update` **não** rodado: precisa de Postgres real + credenciais Supabase.
  Passo de infra do usuário — código está pronto pra aplicar.

## Próximos passos
- Rodar `dotnet ef database update` contra Supabase (porta 5432) quando houver creds.
- Confirmar checkpoint da aula (3 perguntas) sem espiar.
- Fatia 5: pipeline MediatR (Validation/Logging/UnitOfWork behaviors).
