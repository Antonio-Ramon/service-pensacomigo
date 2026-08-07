# 06 — Post: listar / abrir + visualizações

**What to build:** Leitor anônimo lista os posts ordenados por data e abre um post pelo slug (com autor e tags). Abrir incrementa a contagem de visualizações.

**Blocked by:** 05 — Post CRUD.

**Status:** done (código; testes de integração não rodaram aqui — sem Docker)

- [x] Listar posts via **Gridify** (anônimo): `ListarPostsQuery : GridifyQuery`, filtro/ordenação/paginação da querystring, resposta no envelope `Pagina<PostResumoResponse>` (`{ items, totalItems }`); default `OrderBy` = `dataCriacao desc`. `GridifyMapper<Post>` faz whitelist de `titulo`/`slug`/`autor`/`tag`/`dataCriacao` — `tag` mapeia a coleção (`p.Tags.Select(t => t.Slug)`) e vira `EXISTS` na junção. Ver arquitetura §7.1 / Decisão #19.
- [x] Abrir post por slug com autor + tags (anônimo) — `ObterPorSlugAsync` com `Include(Autor)/Include(Tags)` + `AsNoTracking`
- [x] Abrir incrementa `qtd_visualizacoes` — `ExecuteUpdateAsync(SetProperty(p => p.QtdVisualizacoes, p => p.QtdVisualizacoes + 1))`: `coluna = coluna + 1` no banco, sem *lost update*. Sem dedup por visitante (`ponytail:` no handler marca o upgrade)
- [x] Teste de integração: abrir por slug incrementa o contador; 404 em slug inexistente; listagem devolve o envelope e respeita `filter=tag=` e a ordem default — *escritos, não executados aqui (sem Docker)*

**Decisões:**
- `AbrirPostCommand` é **`ICommand`**, não `IQuery`, mesmo sendo `GET`: o critério firmado na issue 04 é efeito colateral, não escrita no Postgres. Verbo HTTP e marcador CQRS são contratos diferentes. Efeito: o GET não é idempotente — sem retry automático nem cache agressivo em cima dele.
- `[Authorize]` continua na classe do `PostsController`; os dois `GET` abrem exceção com `[AllowAnonymous]` (que vence o atributo da classe). O padrão seguro fica na classe.
- Dois shapes de resposta: `PostResumoResponse` (card do feed, **sem** o jsonb do conteúdo) e `PostDetalheResponse` (conteúdo + autor + tags).
