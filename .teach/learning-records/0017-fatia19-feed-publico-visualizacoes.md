# 0017 — Fatia 19: feed público, rota por slug e incremento atômico

**Data:** 2026-08-07 · **Aula:** [0019](../lessons/0019-listar-abrir-visualizacoes.html) · **Ticket:** 06 (fechado no código)

## O que foi aprendido

- **`[AllowAnonymous]` na ação vence `[Authorize]` na classe — e não o contrário.** Padrão
  seguro fica na classe, exceções públicas nas ações. O inverso (classe anônima + ação
  protegida) não protege nada.
- **`{slug}` e `{id:guid}` convivem.** Verbos diferentes já bastariam; além disso a
  constraint `:guid` torna a rota mais específica, então o roteador só cai na genérica
  quando o valor não é um Guid.
- **`GET` com efeito colateral é `ICommand`.** O critério da Fatia 17 (efeito colateral,
  não "escreve no Postgres") vale mesmo contra a intuição do verbo HTTP. Verbo = contrato
  com o cliente; marcador CQRS = instrução ao `UnitOfWorkBehavior`. Consequência: esse GET
  não é idempotente — nada de retry automático nem cache agressivo.
- **`ExecuteUpdateAsync` para contador.** `SetProperty(p => p.Qtd, p => p.Qtd + 1)` — o
  segundo argumento é lambda sobre a entidade, o que gera `coluna = coluna + 1` e evita o
  *lost update* do `++` em memória. Não passa pelo change tracker (objeto em memória fica
  com o valor velho) e executa na hora, fora do `SaveChanges`.
- **Listar e abrir são shapes diferentes**, não um o resumo do outro: `PostResumoResponse`
  deixa o jsonb do conteúdo de fora (20 cards não carregam corpo de post);
  `PostDetalheResponse` traz conteúdo + autor + tags via `Include`, com `AsNoTracking`.
- **GridifyMapper sobre coleção**: `.AddMap("tag", p => p.Tags.Select(t => t.Slug))` vira
  `EXISTS` na junção — filtro por tema sem endpoint novo. Mapper segue sendo whitelist.
  `OrderBy` default `dataCriacao desc` (feed cronológico + paginação estável).

## Estado

Build verde (8 proj, 0 erro), 18 testes unitários verdes. 4 testes de integração novos
(abrir + contador, 404 de slug inexistente, filtro por tag, ordem default) **escritos e
compilando, não executados** — sem Docker nesta máquina.

## Próximo

Ticket 07 — comentários: anônimo com nome, 1 nível de resposta, rate limit por
`viewer_hash` (429) e filtro de palavrão. Primeiro caso com estado em memória e primeira
regra de moderação.
