# 0016 — Fatia 18: editar/deletar post, change tracker e dono do recurso

**Data:** 2026-08-06 · **Aula:** [0018](../lessons/0018-editar-deletar-change-tracker.html) · **Ticket:** 05 (fechado)

## O que foi aprendido

- **Editar não constrói entidade.** Carrega rastreado (`ObterParaEdicaoAsync`, sem
  `AsNoTracking`), muta propriedade, fim. Não existe `repo.Atualizar()`: o change tracker
  compara com a foto do carregamento e o `UnitOfWorkBehavior` commita.
- **`Include(p => p.Tags)` é obrigatório na edição N:N.** O EF só remove da junção o que
  carregou; sem o Include o diff vira "só inserções" e as tags antigas nunca saem.
  Atribuir `post.Tags = vinculadas` gera o delta (DELETE + INSERT em `post_tags`).
- **Slug congelado por omissão**: sem campo no `EditarPostRequest` e sem escrita no handler.
  Campo ausente no DTO é a validação mais barata.
- **Não-dono → 404, não 403.** 403 confirma a existência do recurso e permite enumerar Ids.
  A checagem mora no handler (regra de negócio), não no `[Authorize]` (identidade).
- **Validator genérico**: `IPostEscrita` + `PostEscritaValidator<T> where T : IPostEscrita`,
  fechado por duas subclasses de uma linha. `record` implementa a interface de graça
  (parâmetros posicionais já geram os `get`); `AddValidatorsFromAssembly` ignora genérica
  aberta/abstrata, daí as subclasses precisarem existir.
- **Delete físico**: só `db.Posts.Remove`. Cascata no schema leva comentários, likes e junção;
  a Tag sobrevive. `ICommand<Unit>` = o "void" do MediatR; controller devolve 204.

## Estado

Build verde (8 proj, 0 erro), 18 testes unitários verdes. 3 testes de integração novos
(edita/404 de intruso/deleta) **escritos mas não executados** — sem Docker nesta máquina.

## Próximo

Ticket 06 — listar posts (Gridify, aula 13) e abrir por slug com contador de visualizações
desnormalizado.
