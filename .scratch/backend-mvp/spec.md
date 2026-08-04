# Spec: Backend MVP — Pensa Comigo

Status: ready-for-agent

## Problem Statement

Antonio e Jéssica querem publicar meditações cristãs num blog autoral, montando cada post a partir de um editor de blocos (texto, imagem, link) com reordenação. Leitores precisam poder ler, comentar, curtir e ter suas visualizações contadas — tudo sem criar conta. Hoje só existe o domínio inicial em código (`Post`, `Usuario`, `Comentario`, `Like`, `Tag`, `Bloco`) e um `Program.cs` template; não há persistência, casos de uso, API nem autenticação. Falta o backend inteiro que dá vida a esse fluxo.

## Solution

Uma API REST em .NET 10 (Clean Architecture + CQRS via MediatR + Vertical Slicing) que expõe os casos de uso de posts, comentários, likes, tags, visualizações e autenticação. Persistência em PostgreSQL gerenciado pelo Supabase; imagens no Supabase Storage, enviadas pelo próprio backend. Autores logam pelo Google (frontend faz o OAuth, backend valida e emite JWT próprio); só emails no seed são admins. Leitores interagem anonimamente, com moderação automática de comentários (rate limit + filtro de palavrão). A API é consumida por um frontend Angular (fora deste escopo).

## User Stories

**Autenticação e autores**

1. Como autor, quero logar com minha conta Google, para publicar sem gerenciar senha.
2. Como autor, quero que meu `google_id` e foto sejam preenchidos no primeiro login, para meu perfil refletir a conta Google.
3. Como sistema, quero recusar o login de qualquer email fora do seed, para que só admins autorizados acessem a área de escrita.
4. Como autor, quero receber um JWT próprio após o login, para autenticar as chamadas seguintes.
5. Como autor, quero obter meu perfil, para exibir nome e foto na interface.

**Posts**

6. Como autor, quero criar um post com título, imagem de capa, tags e conteúdo em blocos, para publicar uma meditação.
7. Como autor, quero que o slug seja gerado do título na criação, para ter uma URL amigável sem digitá-la.
8. Como autor, quero que o slug permaneça fixo mesmo se eu editar o título, para não quebrar links já compartilhados.
9. Como sistema, quero resolver colisão de slug com sufixo numérico (`-2`, `-3`), para garantir unicidade.
10. Como sistema, quero calcular o tempo de leitura ao salvar o post, para exibi-lo ao leitor.
11. Como autor, quero atualizar um post existente (título, capa, tags, conteúdo), para corrigir ou melhorar a meditação.
12. Como autor, quero deletar um post, para remover conteúdo que não deve mais existir.
13. Como leitor, quero listar posts com paginação/filtro/ordenação (Gridify), para navegar as meditações — default ordenado por data.
14. Como leitor, quero abrir um post pelo slug, para ler a meditação completa.
15. Como leitor, quero que abrir um post incremente a contagem de visualizações, para refletir a popularidade.

**Conteúdo em blocos**

16. Como autor, quero adicionar blocos de texto rich-text, para escrever a meditação com formatação.
17. Como autor, quero adicionar blocos de imagem (referenciando o Storage), para ilustrar a meditação.
18. Como autor, quero adicionar blocos de link com metadados Open Graph, para citar conteúdo externo.
19. Como autor, quero definir a ordem dos blocos, para controlar a sequência de exibição.

**Imagens**

20. Como autor, quero enviar a imagem para a API, para que ela guarde no storage e me devolva o path e a URL pública.
21. Como autor, quero guardar o path/URL da imagem no bloco ou na capa, para referenciá-la no post.

**Comentários**

22. Como leitor, quero comentar num post informando meu nome, para participar sem criar conta.
23. Como leitor, quero responder a um comentário (1 nível), para conversar sobre a meditação.
24. Como sistema, quero proibir resposta de resposta (2º nível), para manter a árvore rasa.
25. Como sistema, quero limitar a 5 comentários por visitante por minuto, para conter spam.
26. Como sistema, quero bloquear comentários com palavrão, para manter o tom do blog.
27. Como leitor, quero que meu comentário limpo apareça imediatamente, para não esperar aprovação.
28. Como leitor, quero listar os comentários aprovados de um post (com respostas) com paginação (Gridify), para ler a conversa.
29. Como admin, quero esconder ou deletar um comentário publicado, para moderar conteúdo indevido.

**Likes**

30. Como leitor, quero curtir um post, para expressar apreço sem criar conta.
31. Como sistema, quero deduplicar curtidas por visitante (`viewer_hash`), para impedir curtida repetida.
32. Como leitor, quero descurtir um post, para desfazer minha curtida.
33. Como sistema, quero manter `qtd_curtidas` desnormalizado e atômico com o Like, para leitura rápida sem `COUNT`.

**Tags**

34. Como autor, quero criar tags, para classificar posts.
35. Como leitor, quero listar tags (via Gridify, por consistência — filtro/página raramente usados aqui), para navegar por tema.
36. Como autor, quero associar tags a um post (N:N), para categorizá-lo.

## Implementation Decisions

**Camadas / estrutura**
- Clean Architecture: `Domain` → `Application` → (`Persistence`, `Shared`) → `Web`. CQRS via MediatR; Vertical Slicing por caso de uso em `Application/UseCases/<Módulo>/`.
- Cada Command/Query traz Handler, Validator e Response coesos no slice.

**Acesso a dados**
- Repository pattern mantido, **um repositório por raiz de agregado**: `IPostRepository`, `IComentarioRepository`, `IUsuarioRepository`, `ITagRepository`. Contratos no Domain, implementações no Persistence sobre `PensaComigoDbContext`. Sem repositório para `Like` (manipulado dentro do fluxo do Post) nem `post_tags` (junção via navegação `Post.Tags`).
- Métodos de repositório refletem casos de uso reais (ex.: `ExisteSlug`, `ObterPorSlugComAutorETags`), encapsulando `Include`/projeção.

**Pipeline MediatR**
- Três behaviors: `ValidationBehavior` (FluentValidation antes do Handler), `LoggingBehavior` (request/response + tempo), `UnitOfWorkBehavior` (abre transação e faz commit atômico **apenas nos Commands**, garantindo Like + `qtd_curtidas` na mesma transação).

**Erros**
- Regras de negócio sinalizam via exceções tipadas de domínio; um `ExceptionHandlingMiddleware` no Web mapeia para status HTTP (404 não encontrado, 422 regra de negócio, 429 rate limit). Erros de validação do FluentValidation caem no mesmo middleware. Controllers ficam magros.

**Conteúdo em blocos (jsonb)**
- `Bloco` é modelo **flat** (todos os campos possíveis coexistem; `TipoBloco` indica quais valem). Já existe no Domain.
- `Post.Conteudo` (`List<Bloco>`) persiste em coluna `jsonb` via **conversor manual do EF** (`HasConversion` + `JsonSerializer`), tratado como blob inteiro. **Sem índice GIN.**

**Autenticação / autorização**
- Frontend executa Google OAuth e envia o token; backend valida a assinatura, localiza o usuário existente por email e emite **JWT próprio** (JwtBearer já referenciado). Endpoints de escrita/moderação exigem esse JWT.
- Apenas usuários do seed (Antonio, Jéssica) são admins; login de email fora do seed é recusado (não cria usuário). `usuarios.is_admin` default `false`. Novos autores entram via seed/migration.

**Slug**
- `GeradorSlug` no Shared: normaliza título (remove acento/pontuação, minúsculo, espaço → `-`), gera na criação, **congela** depois; colisão resolve com sufixo `-N`.

**Visualizações**
- Incremento **cru** de `qtd_visualizacoes` a cada abertura de post, sem proteção contra refresh, sem tabela dedicada. Ponto de upgrade (dedup por cache) marcado com comentário `ponytail:`.

**Comentários — moderação automática (estilo YouTube)**
- Rate limit: máx. 5 comentários por `viewer_hash` em janela deslizante de 1 min → 429. Estado em cache de memória (upgrade Redis marcado com `ponytail:`).
- Filtro de palavrão: lista de termos proibidos (config no Shared); match → **bloqueia** com erro de validação. Nada sujo entra no banco.
- Comentário que passa nos dois filtros publica na hora (`aprovado = true`). Regra "1 nível" garantida no Handler/Validator, não no schema.
- `aprovado` serve ao admin para esconder manualmente; admin também deleta.

**Likes**
- Dedup por constraint única `(post_id, viewer_hash)`. Curtir insere Like e incrementa `qtd_curtidas`; descurtir remove e decrementa — atômico via `UnitOfWorkBehavior`.

**Imagens**
- Upload **pelo backend** (multipart): `Application/Storage.IStorage.EnviarAsync(path, stream, contentType)`; o endpoint valida (extensão, tamanho, content-type da whitelist), monta o path a partir da claim e repassa ao Supabase Storage. Resposta `{ path, url }` (bucket público para leitura).

**Persistence / banco**
- EF Core (Npgsql) sobre PostgreSQL do Supabase. SSL obrigatório. Conexão direta (5432) para migrations; pooled Supavisor (6543) para runtime.
- `EntityTypeConfiguration` por entidade; migrations e seed (`UsuariosSeed`: Antonio, Jéssica) no Persistence.

**API**
- REST versionada `/api/v1`, controllers magros que só despacham para os Handlers via MediatR, retornando `ActionResult<T>`. OpenAPI + Swagger UI. Módulos: Auth, Posts, Comentários, Likes, Tags, Usuários.

**Listagem e paginação (Gridify) — padrão project-wide**
- **Todo endpoint de listagem** usa **Gridify** (filtro/ordenação/paginação dinâmicos via querystring) e responde no **envelope padrão `Paging<T>`** (`{ items, totalItems }`), **nunca** lista crua. Espelha o `service-escolaweb`.
- Query de listagem **herda de `GridifyQuery`** (traz `Page`, `PageSize`, `OrderBy`, `Filter` — aparecem sozinhos no Swagger). Um **`GridifyMapper` por entidade** faz whitelist dos campos filtráveis/ordenáveis (cliente não vê nome de coluna cru). O repositório aplica filtro+ordem+página no `IQueryable` e retorna `(Query, TotalItems)`.
- **Aplica-se ao projeto inteiro** — inclusive em lookup pequeno como **Tags**, onde é overkill (parâmetro que quase ninguém usa), mas mantido pela **consistência** do contrato de listagem. Endpoints afetados: listar posts (US 13), listar comentários (US 28), listar tags (US 35). Detalhe em `architecture-pensa-comigo.md §7.1` e Decisão #19.

**Schema (contrato)**
- Tabelas `usuarios`, `posts`, `tags`, `post_tags`, `comentarios`, `likes` conforme `architecture-pensa-comigo.md §5.4` (com `is_admin default false` e sem índice GIN). Contadores desnormalizados em `posts`. `comentarios.parent_id` auto-referência para 1 nível. `likes` com unique `(post_id, viewer_hash)`.

## Testing Decisions

- **O que faz um bom teste:** exercita comportamento externo observável, não detalhe de implementação. Um teste não deve conhecer repositórios nem estrutura interna dos Handlers — só entradas e saídas do sistema.
- **Seam principal (integração):** a fronteira HTTP da API, via `WebApplicationFactory<Program>` + **Testcontainers** (Postgres real). Exercita cada fluxo ponta a ponta: criar/editar/listar/abrir post, comentar/responder/moderar, curtir/descurtir, criar/listar tag, login. Valida o `jsonb`, a unique `(post_id, viewer_hash)`, geração/congelamento de slug e atomicidade dos contadores contra Postgres de verdade. Requer `public partial class Program {}` no fim do `Program.cs`.
- **Seams menores (unit, lógica pura):** `GeradorSlug` (normalização + colisão), cálculo de `TempoLeitura`, filtro de palavrão, avaliação do rate limit. Funções isoláveis, testadas direto sem HTTP/DB.
- **Regra de quantidade de seams:** preferir o seam HTTP; só descer a unit onde a lógica é pura e barata de isolar. Nenhum seam novo além desses.
- **Prior art:** ainda não há testes no repo — estes estabelecem o padrão. `PensaComigo.UnitTests` (xUnit) para os seams menores; `PensaComigo.IntegrationTests` para o seam HTTP.

## Out of Scope

- Frontend Angular (consome esta API; documentado à parte).
- Proteção sofisticada de visualizações (dedup por cache/tabela) — fica no incremento cru.
- Cache distribuído (Redis) para rate limit — memória local por ora.
- Busca full-text dentro dos blocos (por isso sem índice GIN).
- Contas e perfis de leitor — leitor interage anonimamente.
- Notificações, RSS, analytics, painel administrativo além dos endpoints de moderação.
- Renovação/refresh sofisticado de token além da emissão do JWT no login.

## Further Notes

- Ordem de implementação combinada: **Domain → Persistence (Context + Configurations + Migration + Seed) → Application (behaviors + use cases) → Web (controllers + middleware + auth) → testes.**
- Decisões completas e justificativas em `architecture-pensa-comigo.md §8` (Decisões #1–19) e na memória do projeto (`decisoes-arquitetura`).
- Detalhes idiomáticos resolvidos na implementação com padrão .NET: versionamento por atributo de rota, `ActionResult<T>`.
