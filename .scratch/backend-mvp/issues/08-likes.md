# 08 — Likes

**What to build:** Leitor anônimo curte um post e pode descurtir. Curtidas são deduplicadas por visitante (`viewer_hash`) e o contador `qtd_curtidas` é mantido desnormalizado e atômico junto do Like — leitura rápida sem `COUNT`.

**Blocked by:** 05 — Post CRUD.

**Status:** ready-for-agent

- [ ] Curtir: insere Like + incrementa `qtd_curtidas`, atômico via `UnitOfWorkBehavior`
- [ ] Descurtir: remove Like + decrementa
- [ ] Dedup por unique `(post_id, viewer_hash)` — curtida repetida não conta
- [ ] Teste de integração: curtir duas vezes o mesmo `viewer_hash` mantém contador em 1; descurtir zera; atomicidade contra Postgres real

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — só curtir/descurtir, sem endpoint de listagem.
