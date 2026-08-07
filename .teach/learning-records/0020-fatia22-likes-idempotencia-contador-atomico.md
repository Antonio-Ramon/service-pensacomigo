# 0020 — Fatia 22: idempotência, unicidade no banco e contador atômico

**Data:** 2026-08-07 · **Aula:** [0022](../lessons/0022-likes-idempotencia-contador-atomico.html) · **Ticket:** 08 (fechado)

## O que foi aprendido

- **Idempotência é uma decisão de API, não um detalhe.** Curtir de novo e descurtir o que não
  foi curtido devolvem **204**, o mesmo status da chamada que fez efeito. 404 no descurtir
  obrigaria o cliente a saber o estado antes de agir — e a rede reenvia requisições sozinha.
- **`if` não garante unicidade; constraint garante.** `ExisteAsync` é check-then-act: entre o
  `SELECT` e o `INSERT` cabe outra requisição inteira. O `if` é conveniência (transforma o caso
  comum num 204 limpo), o índice único `(post_id, viewer_hash)` é a garantia. A corrida perdida
  vira `DbUpdateException` → **409**, que o `GlobalExceptionHandler` (Fatia 6) já traduzia.
- **Contador desnormalizado nos dois sentidos**: `ExecuteUpdateAsync(SetProperty(p => p.QtdCurtidas,
  p => p.QtdCurtidas + delta))`. Ler-somar-gravar pelo change tracker perderia contagem com dois
  visitantes simultâneos (*lost update*). Um método só com `delta` (+1/-1) em vez de dois.
- **Guarda no banco em vez de `if` no C#**: `Where(p => p.Id == id && p.QtdCurtidas + delta >= 0)`
  — descurtir a mais afeta zero linhas em vez de deixar o contador negativo.
- **Toda otimização que pula uma camada perde as garantias daquela camada.** `ExecuteUpdate` grava
  na hora, fora do `SaveChanges` do `UnitOfWorkBehavior`: ganhamos atomicidade por linha e perdemos
  a atomicidade do caso de uso (são duas transações). Aceito conscientemente, marcado com
  `ponytail:`; upgrade = transação explícita no behavior. Contador levemente adiantado dói menos
  que curtida perdida — essa é a escolha, e ela é do domínio.
- **Modelagem certa dispensa mecanismo.** Comentário precisou de `LimitadorDeComentarios` porque um
  visitante escreve infinitos. Curtida já está limitada a uma por visitante por post — pela
  constraint. Nenhum rate limit foi escrito nesta fatia.
- **O `viewer_hash` da Fatia 20 foi promovido**: era chave de balde em memória, virou coluna com
  índice único. Mesmo cálculo no servidor (`HashVisitante.De(HttpContext)`) pelo mesmo motivo de
  sempre — se viesse do cliente, trocar um caractere zeraria a dedup.

## Estado

Build verde (8 proj, 0 erro, 1 warning pré-existente), 33 testes unitários verdes (a fatia não
trouxe função pura nova). 5 testes de integração novos **escritos e compilando, não executados** —
sem Docker nesta máquina. **Ticket 08 fechado no código** (4 de 4 itens).

## Próximo

Ticket 09 em diante — conferir `.scratch/backend-mvp/spec.md`. Candidatos de conceito ainda não
vistos: transação explícita (fecharia a ressalva desta fatia), cache de leitura, ou o front
consumindo a API.
