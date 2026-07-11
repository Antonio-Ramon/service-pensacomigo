# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Comandos

Build/test operam sobre `service-pensacomigo.slnx` (solução em formato XML novo — abra com `dotnet` normalmente).

```bash
dotnet build                                              # compila toda a solução
dotnet run --project src/PensaComigo.Web                  # sobe a API (Swagger em Development)
dotnet test                                               # roda unit + integration tests
dotnet test tests/PensaComigo.UnitTests                   # só um projeto de teste
dotnet test --filter "FullyQualifiedName~NomeDoTeste"     # um teste específico (xUnit)
dotnet ef migrations add <Nome> -p src/PensaComigo.Persistence -s src/PensaComigo.Web
dotnet ef database update -p src/PensaComigo.Persistence -s src/PensaComigo.Web
```

Stack: **.NET 10**, EF Core 10 + Npgsql (PostgreSQL), MediatR 14, FluentValidation 12, JWT Bearer, Swashbuckle/Swagger, xUnit. Nullable e ImplicitUsings habilitados em todos os projetos. Idioma do domínio e commits: **pt-br**.

## Agent skills

### Issue tracker

Issues e specs vivem como markdown em `.scratch/<feature-slug>/`. See `docs/agents/issue-tracker.md`.

### Triage labels

Cinco labels canônicos padrão (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: `CONTEXT.md` na raiz + `docs/adr/`. See `docs/agents/domain.md`.

## Comunicação

Quando estiver falando comigo, sacrifique a gramática em favor da concisão.

## Arquitetura

Clean Architecture em 5 projetos. Direção de dependência (o que importa):

- **Domain** — núcleo, sem dependências. Entities (`Post`, `Usuario`, `Comentario`, `Like`, `Tag`), ValueObjects, Enums.
- **Application** — referencia só Domain. CQRS via MediatR (handlers, commands/queries) e validação via FluentValidation. É a camada central onde as regras de caso de uso vivem.
- **Persistence** — referencia Application + Domain. EF Core `DbContext`, configurações e migrations. Provider PostgreSQL.
- **Shared** — referencia Application + Domain. Contratos/DTOs cruzados.
- **Web** — host ASP.NET Core (controllers em `Controllers/`, ainda a criar). Referencia Application + Persistence + Shared. `Program.cs` é minimalista; DI, auth e pipeline ainda estão sendo montados.

## Convenções específicas que precisam de contexto

- **Blocos de conteúdo (`ValueObjects/Bloco.cs`)**: o `Conteudo` de um `Post` é uma `List<Bloco>` persistida como **coluna jsonb** (complex type do EF 10). `Bloco` usa modelo *flat*: todos os campos possíveis (texto/imagem/link) coexistem e `TipoBloco` indica quais estão preenchidos — não crie hierarquia de subtipos.
- **Contadores desnormalizados**: `Post.QtdCurtidas`, `QtdVisualizacoes` são materializados na entidade; ao mexer em Like/View, atualize o contador no handler correspondente.
- **Campos calculados no handler, não na entidade**: ex. `TempoLeitura` é calculado na Application na criação/edição do post.
