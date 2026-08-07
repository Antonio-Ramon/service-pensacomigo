# 05 — Post: criar / editar / deletar

**What to build:** Autor autenticado monta uma meditação com título, capa, tags e conteúdo em blocos (texto rich-text, imagem, link com Open Graph), com ordem definida. O slug nasce do título na criação e **congela** depois; o tempo de leitura é calculado ao salvar. Autor edita e deleta o próprio post.

**Blocked by:** 03 — Tags (associar tags), 04 — Imagens (capa/bloco de imagem referenciam path).

**Status:** ready-for-agent

- [x] Criar post: título, capa (path), tags (N:N via `Post.Tags`), conteúdo `List<Bloco>` (texto/imagem/link, flat, com ordem)
- [x] `GeradorSlug` ~~no Shared~~ **em `Application/Common`**: normaliza (sem acento/pontuação, minúsculo, espaço→`-`), colisão resolve com sufixo `-N` [unit] — *Shared referencia Application, então a Application não enxergaria de volta*
- [x] `CalculadoraTempoLeitura` em `Application/Common` [unit] — plugada no `CriarPostCommandHandler`
- [x] Editar post (título, capa, tags, conteúdo) — slug permanece fixo
- [x] Deletar post (cascata leva comentários/likes/junção; a tag sobrevive)
- [x] Escrita/edição/delete exigem JWT — `[Authorize]` no controller; não-autor recebe **404** (não 403: 403 confirma que o post existe)
- [x] Teste de integração: cria post e valida jsonb + slug congelado + colisão de slug contra Postgres real — *escrito, não executado aqui (sem Docker nesta máquina)*

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — este ticket é criar/editar/deletar; a listagem de posts (com Gridify) é a issue 06.
