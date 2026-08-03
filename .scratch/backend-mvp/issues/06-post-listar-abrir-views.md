# 06 — Post: listar / abrir + visualizações

**What to build:** Leitor anônimo lista os posts ordenados por data e abre um post pelo slug (com autor e tags). Abrir incrementa a contagem de visualizações.

**Blocked by:** 05 — Post CRUD.

**Status:** ready-for-agent

- [ ] Listar posts via **Gridify** (anônimo): `ListarPostsQuery : GridifyQuery`, filtro/ordenação/paginação da querystring, resposta no envelope `Paging<T>` (`{ items, totalItems }`); default `OrderBy` = data desc. `GridifyMapper<Post>` faz whitelist dos campos filtráveis (ex.: título, tag, data). Ver arquitetura §7.1 / Decisão #19. *(Caso principal onde o padrão paga por si — lista que cresce.)*
- [ ] Abrir post por slug com autor + tags (anônimo)
- [ ] Abrir incrementa `qtd_visualizacoes` (incremento cru, sem dedup — `ponytail:` marca upgrade)
- [ ] Teste de integração: abrir por slug incrementa o contador; listagem devolve envelope `Paging<T>` e respeita `Filter`/`OrderBy`/`Page`
