# Pensa Comigo — API

Backend do blog de meditações cristãs **Pensa Comigo**: autores publicam posts montados em blocos
(texto, imagem, link); leitores leem, comentam e curtem **sem criar conta**.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Supabase-4169E1?logo=postgresql&logoColor=white)](https://supabase.com/)
[![MediatR](https://img.shields.io/badge/MediatR-14-6E4AFF)](https://github.com/jbogard/MediatR)
[![FluentValidation](https://img.shields.io/badge/FluentValidation-12-2E7D32)](https://docs.fluentvalidation.net/)
[![xUnit](https://img.shields.io/badge/tests-xUnit%20%2B%20Testcontainers-1F8ACB)](https://xunit.net/)
[![Arquitetura](https://img.shields.io/badge/arquitetura-Clean%20%2B%20CQRS-informational)](docs/architecture-pensa-comigo.md)
[![Status](https://img.shields.io/badge/MVP-completo-success)](.scratch/backend-mvp/issues)

---

## Sumário

- [O que a API faz](#o-que-a-api-faz)
- [Stack](#stack)
- [Arquitetura](#arquitetura)
- [Como rodar](#como-rodar)
- [Configuração](#configuração)
- [Endpoints](#endpoints)
- [Listagens: filtro, ordenação e paginação](#listagens-filtro-ordenação-e-paginação)
- [Erros](#erros)
- [Testes](#testes)
- [Migrations](#migrations)
- [Documentação](#documentação)

---

## O que a API faz

| Domínio | Resumo |
|---|---|
| **Auth** | Frontend faz o OAuth do Google e envia o token; a API valida a assinatura e emite **JWT próprio** (8h). Só emails do seed logam — não cria usuário. |
| **Posts** | CRUD do autor + feed público. Conteúdo é uma lista de blocos gravada em coluna **`jsonb`**. Slug gerado na criação e **congelado** depois; `tempo_leitura` calculado no caso de uso. |
| **Tags** | Criar (autenticado) e listar (anônimo). Relação N:N com posts. |
| **Comentários** | Anônimos, com moderação automática: **rate limit** de 5/min por visitante e filtro de palavrão. Uma resposta de profundidade. Admin oculta ou apaga. |
| **Curtidas** | Anônimas e idempotentes, deduplicadas por visitante via índice único `(post_id, viewer_hash)`. |
| **Imagens** | Upload multipart pelo backend para o Supabase Storage, com whitelist de extensão/content-type e limite de 5 MB. |
| **Visualizações** | Contador incrementado na abertura do post, atômico no banco (`coluna = coluna + 1`). |

O visitante anônimo é identificado por um **`viewer_hash`** calculado no servidor (SHA-256 de IP +
User-Agent). Aproximado por natureza — suficiente para dedup de curtida e rate limit de comentário.

## Stack

.NET 10 · ASP.NET Core (controllers) · EF Core 10 + Npgsql (PostgreSQL/Supabase) · MediatR 14 ·
FluentValidation 12 · Gridify 2.19 · JWT Bearer · Swashbuckle/Swagger · xUnit + Testcontainers.

`Nullable` e `ImplicitUsings` habilitados em todos os projetos. Domínio, commits e mensagens de
erro em **pt-br**.

## Arquitetura

Clean Architecture em 5 projetos — a seta é sempre para dentro:

```
Web ──► Application ──► Domain
 │            ▲
 └► Persistence ┘        Shared ──► Application, Domain
```

| Projeto | Papel |
|---|---|
| `PensaComigo.Domain` | Núcleo sem dependências: entidades, value objects, exceções e interfaces de repositório. |
| `PensaComigo.Application` | Casos de uso (CQRS via MediatR), validators, behaviors e funções puras. Só conhece o Domain. |
| `PensaComigo.Persistence` | `DbContext`, configurations, migrations e implementações de repositório. |
| `PensaComigo.Shared` | Contratos cruzados (envelope de erro, DTOs). |
| `PensaComigo.Web` | Host: controllers magros, autenticação, Swagger e as implementações que dependem de libs externas (Google, Supabase). |

**Pipeline do MediatR** (de fora para dentro): `Logging` → `Validation` → `UnitOfWork` → handler.
O `UnitOfWorkBehavior` dá `SaveChanges` **só em Command e só depois do handler passar** — a Query
nunca commita. Command e Query se distinguem pelos marcadores `ICommand` / `IQuery`.

Detalhes e decisões numeradas em [`docs/architecture-pensa-comigo.md`](docs/architecture-pensa-comigo.md).

## Como rodar

Pré-requisitos: **SDK do .NET 10** e um PostgreSQL acessível (Supabase ou local). Docker só é
necessário para os testes de integração.

```bash
dotnet build                                # compila a solução (service-pensacomigo.slnx)
dotnet run --project src/PensaComigo.Web    # sobe a API
```

Em `Development`, o Swagger UI abre em `/swagger` já com "try it out" ligado e botão **Authorize**
para colar o JWT.

## Configuração

A aplicação **não sobe** sem as chaves do Supabase (validação de options no start) — isso é
proposital. Use user-secrets, nunca o `appsettings.json`:

```bash
cd src/PensaComigo.Web
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=postgres;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "Jwt:Key" "<chave simétrica longa>"
dotnet user-secrets set "Google:ClientId" "<client id do OAuth>"
dotnet user-secrets set "Supabase:Url" "https://<projeto>.supabase.co"
dotnet user-secrets set "Supabase:ServiceRoleKey" "<service role key>"
dotnet user-secrets set "Visitantes:Pepper" "<segredo longo e aleatório>"
```

| Chave | Para quê |
|---|---|
| `ConnectionStrings:Default` | Postgres. Porta **5432** (conexão direta) para migrations; **6543** (pooler) para runtime. |
| `Jwt:Issuer` / `Jwt:Audience` / `Jwt:Key` | Emissão e validação do JWT próprio. |
| `Google:ClientId` | Audiência esperada no token do Google. |
| `Supabase:Url` / `ServiceRoleKey` / `Bucket` | Upload de imagens. |
| `Visitantes:Pepper` | Segredo do HMAC que identifica o leitor anônimo. Sem ele, curtir/comentar estoura. **Trocar o valor invalida os `viewer_hash` já gravados.** |
| `ProxiesConfiaveis` | IPs ou CIDRs do proxy/ingress cujo `X-Forwarded-For` é aceito (`ProxiesConfiaveis__0=10.0.0.0/8` via env var). Vazio = header ignorado, correto em localhost. Atrás de proxy é obrigatório preencher: sem isso todo visitante colapsa numa identidade só. |

## Endpoints

Todos sob `/api/v1`. Versionamento é convenção de rota — sem lib até existir um v2.

| Método | Rota | Acesso |
|---|---|---|
| `POST` | `/auth/login` | anônimo |
| `GET` | `/usuarios/me` | autenticado |
| `GET` | `/posts` | anônimo |
| `GET` | `/posts/{slug}` | anônimo (incrementa visualizações) |
| `POST` `PUT` `DELETE` | `/posts` · `/posts/{id}` | autor dono |
| `GET` | `/tags` | anônimo |
| `POST` | `/tags` | autenticado |
| `GET` `POST` | `/posts/{postId}/comentarios` | anônimo |
| `PATCH` | `/posts/{postId}/comentarios/{id}/ocultar` | **admin** |
| `DELETE` | `/posts/{postId}/comentarios/{id}` | **admin** |
| `POST` `DELETE` | `/posts/{postId}/curtidas` | anônimo |
| `POST` | `/imagens` | autenticado |

Escrita de post exige o JWT; **não ser o dono devolve 404**, não 403 — 403 vazaria a existência do
recurso. Moderação exige a claim `is_admin=true`; token sem ela toma **403**.

## Listagens: filtro, ordenação e paginação

Toda listagem usa **Gridify** e responde no mesmo envelope, nunca em lista crua:

```json
{ "items": [ ... ], "totalItems": 42 }
```

```http
GET /api/v1/posts?page=1&pageSize=10&orderBy=dataCriacao desc&filter=tag=oracao
```

Cada entidade tem um `GridifyMapper` que funciona como **whitelist** — campo fora dele é ignorado,
e o cliente nunca vê nome de coluna do banco.

## Erros

Envelope único em toda falha:

```json
{
  "successed": false,
  "message": "Não foi possível criar o post.",
  "notifications": [ { "key": "titulo", "message": "Informe o título." } ]
}
```

| Status | Quando |
|---|---|
| `400` | Requisição ilegível (JSON quebrado, guid inválido na rota) |
| `401` | Token ausente, inválido ou expirado |
| `403` | Autenticado, mas sem a claim exigida |
| `404` | Recurso inexistente — ou existente e não seu |
| `409` | Conflito de unicidade no banco |
| `422` | Regra de negócio ou validação de campo |
| `429` | Rate limit de comentários estourado |

## Testes

```bash
dotnet test                                            # tudo
dotnet test tests/PensaComigo.UnitTests                # não precisa de Docker
dotnet test --filter "FullyQualifiedName~CurtidasTests"
```

- **UnitTests** — funções puras: geração/colisão de slug, tempo de leitura, filtro de palavrão,
  janela deslizante do rate limit. Rodam em milissegundos.
- **IntegrationTests** — seam HTTP: `WebApplicationFactory<Program>` + **Testcontainers** sobem a
  API e um **PostgreSQL real**, com a migration aplicada. Exigem **Docker no ar**.

## Migrations

```bash
dotnet ef migrations add <Nome> -p src/PensaComigo.Persistence -s src/PensaComigo.Web
dotnet ef database update     -p src/PensaComigo.Persistence -s src/PensaComigo.Web
```

O seed cria os autores admin (`UsuariosSeed`) — novos autores entram por seed/migration, não por
cadastro.

## Documentação

| Onde | O quê |
|---|---|
| [`docs/architecture-pensa-comigo.md`](docs/architecture-pensa-comigo.md) | Arquitetura completa, schema e decisões numeradas |
| [`docs/adr/`](docs/adr) | Architecture Decision Records |
| [`.scratch/backend-mvp/`](.scratch/backend-mvp) | Spec do MVP e issues fatiadas |
| [`.teach/lessons/`](.teach/lessons) | Aulas que acompanharam a construção, fatia por fatia |
| [`CLAUDE.md`](CLAUDE.md) | Convenções para agentes trabalhando neste repo |
