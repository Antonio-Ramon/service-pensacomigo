# 08 — Likes

**What to build:** Leitor anônimo curte um post e pode descurtir. Curtidas são deduplicadas por visitante (`viewer_hash`) e o contador `qtd_curtidas` é mantido desnormalizado e atômico junto do Like — leitura rápida sem `COUNT`.

**Blocked by:** 05 — Post CRUD.

**Status:** done

- [x] Curtir: insere Like + incrementa `qtd_curtidas` (`ExecuteUpdateAsync`, `coluna = coluna + 1`)
- [x] Descurtir: remove Like + decrementa, com guarda `>= 0` no `Where`
- [x] Dedup por unique `(post_id, viewer_hash)` — curtida repetida é no-op idempotente (204);
      corrida perdida cai no índice único → 409 pelo `GlobalExceptionHandler`
- [x] Teste de integração: curtir duas vezes mantém contador em 1; visitantes distintos somam;
      descurtir zera; descurtir sem ter curtido não fica negativo; post inexistente → 404
      (escritos e compilando; **não executados** — sem Docker nesta máquina)

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — só curtir/descurtir, sem endpoint de listagem.

> **Ressalva conhecida** (`ponytail:` em `CurtirPostCommandHandler`): `ExecuteUpdateAsync` grava fora
> do commit do `UnitOfWorkBehavior`, então o caso de uso são duas transações. Se o INSERT do like
> perder a corrida do índice único, o contador fica 1 acima. Upgrade: transação explícita no behavior.
