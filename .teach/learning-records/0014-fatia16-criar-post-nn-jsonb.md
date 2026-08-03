# 0014 — Fatia 16: CriarPostCommand (N:N + jsonb)

**Data:** 2026-08-03 · **Aula:** `lessons/0016-criar-post-nn-jsonb.html` · **Issue:** 05 — Post (criar/editar/deletar)

## Contexto
Primeiro caso de uso que escreve nas três formas ao mesmo tempo: colunas normais, coluna `jsonb`
(`List<Bloco>`) e linhas de junção (`post_tags`). As funções puras da Fatia 15 entraram no handler.

## O que foi aprendido
- **N:N depende do change tracker, não do Id.** `new Tag { Id = ... }` entra como `Added` → o EF
  tenta `INSERT INTO tags`. Carregar as tags do `DbSet` (estado `Unchanged`) faz o EF emitir só o
  `INSERT INTO post_tags`.
- **`AsNoTracking` é a regra, exceto quando a entidade lida vai participar de uma escrita.**
  `ObterPorIdsAsync` é o único método de repo sem ele — e o comentário no código diz por quê.
- **jsonb é blob**: sem `WHERE`/FK/ordenação por campo de bloco. Critério que ficou:
  *navega/filtra/conta → entidade; lê e grava inteiro → jsonb*.
- `[.. lista.OrderBy(...)]` — collection expression com spread (C# 12), inferindo `List<Bloco>`.
- **Modelo flat cobra o preço na validação**: `RuleForEach` + `switch` expression sobre o enum é
  quem garante que `Tipo=Texto` tenha `Html`. O compilador não garante nada num modelo flat.
- `Except()` do LINQ como diferença de conjuntos: pedido − encontrado = tag inexistente → 404.

## Decisões
- **`CriarPostRequest` separado do `CriarPostCommand`** (mesmo padrão da Fatia 14): `AutorId` sai da
  claim `sub`. Regra consolidada: *todo campo que responde "de quem é isso?" vem da claim*.
- **`ListarSlugsComPrefixoAsync` no `IPostRepository`** — um round-trip (`LIKE 'prefixo%'`) alimenta
  o `ResolverColisao` puro. Assinatura desenhada na Fatia 15, agora com o consumidor real.
- **Ordem do bloco é dado, não posição do array**: grava `OrderBy(b => b.Ordem)`, mas o campo é a
  verdade.
- Tag inexistente → `NaoEncontradoException` (404), não 422: o cliente mandou um id que não existe,
  não uma regra violada.

## A revisitar
- **`Bloco.Id` vem do cliente** (o record tem default `Guid.NewGuid()`, mas o JSON sobrescreve).
  Nada depende disso hoje; se a edição passar a casar blocos por id, vira fronteira de confiança.
- Não há checagem de que `ImagemCapa`/`ImagemPath` apontam pra uma pasta do próprio autor — o path
  é gerado pelo servidor na Fatia 14, mas o cliente pode mandar outro aqui. Vale amarrar no ticket
  de edição.
- Teste de integração escrito (jsonb + junção + colisão + 422), **não rodou**: Docker segue ausente
  nesta máquina desde a Fatia 8.
