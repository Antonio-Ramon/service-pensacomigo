# 03 — Tags

**What to build:** Autor autenticado cria tags; leitor lista tags para navegar por tema.

**Blocked by:** 02 — Auth (criar tag exige autor).

**Status:** done (código; teste não rodou aqui — sem Docker)

- [x] Autor cria tag (autenticado)
- [x] Leitor lista tags (anônimo)
- [x] Teste de integração: criar tag exige JWT; listagem pública devolve as tags — *escrito e compila; não rodou aqui (sem Docker)*

**Feito:** `CriarTagCommand/Handler/Validator` ([Authorize]) + `ListarTagsQuery/Handler` ([AllowAnonymous]) no mesmo `TagsController`. Slug calculado no handler (Normalize FormD → remove acento → regex hífen); colisão de slug → `RegraDeNegocioException` (422) antes do índice único. `ITagRepository` ganhou `ListarAsync` + `ExistePorSlugAsync`. `TagsTests` (401 sem token / 200+slug com token do seed via `IJwtTokenGenerator` da DI / GET anônimo lista).

- [x] **Conformar listagem ao padrão Gridify** (arquitetura §7.1 / Decisão #19): `ListarTagsQuery : GridifyQuery` (virou `class` — `record` não herda de classe comum), controller recebe `[FromQuery]`, resposta no envelope `Pagina<TagResponse>`; `GridifyMapper<Tag>` expondo só `nome`/`slug`; `OrderBy` default `nome` no repositório (paginação estável). `TagsTests` cobre o envelope + `?filter=slug=saude-mental`. **Overkill reconhecido** (Tags é lookup pequeno), mantido pela consistência project-wide.
