# 05 — Post: criar / editar / deletar

**What to build:** Autor autenticado monta uma meditação com título, capa, tags e conteúdo em blocos (texto rich-text, imagem, link com Open Graph), com ordem definida. O slug nasce do título na criação e **congela** depois; o tempo de leitura é calculado ao salvar. Autor edita e deleta o próprio post.

**Blocked by:** 03 — Tags (associar tags), 04 — Imagens (capa/bloco de imagem referenciam path).

**Status:** ready-for-agent

- [x] Criar post: título, capa (path), tags (N:N via `Post.Tags`), conteúdo `List<Bloco>` (texto/imagem/link, flat, com ordem)
- [x] `GeradorSlug` ~~no Shared~~ **em `Application/Common`**: normaliza (sem acento/pontuação, minúsculo, espaço→`-`), colisão resolve com sufixo `-N` [unit] — *Shared referencia Application, então a Application não enxergaria de volta*
- [x] `CalculadoraTempoLeitura` em `Application/Common` [unit] — plugada no `CriarPostCommandHandler`
- [ ] Editar post (título, capa, tags, conteúdo) — slug permanece fixo
- [ ] Deletar post
- [ ] Escrita/edição/delete exigem JWT
- [ ] Teste de integração: cria post e valida jsonb + slug congelado + colisão de slug contra Postgres real

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — este ticket é criar/editar/deletar; a listagem de posts (com Gridify) é a issue 06.
