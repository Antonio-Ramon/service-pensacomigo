# 0001 — Fatia 1: Mapeamento EF Core + DbContext

**Data:** 2026-07-11
**Aula:** lessons/0001-ef-core-mapping.html

## O que foi coberto
- Modelo mental do ORM: classe→tabela, propriedade→coluna, navegação→FK.
- Por que Fluent API (classes `IEntityTypeConfiguration`) em vez de Data Annotations:
  Domain não pode depender de banco (Clean Architecture).
- Anatomia de uma Configuration: `ToTable`, `HasKey`, `Property/HasColumnName`,
  `IsRequired`, `HasIndex().IsUnique()`, defaults.
- Relacionamentos: 1:N, auto-referência (Comentario), N:N (post_tags), unique composto (likes).
- `DbContext` + `ApplyConfigurationsFromAssembly`.

## Entregue no código
- `PensaComigoDbContext` + 5 configurations (Usuario, Post, Tag, Comentario, Like).
- Schema snake_case explícito batendo com architecture §5.4. Build verde.
- jsonb do Post já incluído no código (com ValueComparer), mas **explicação fica pra Fatia 2**.

## Zona de desenvolvimento proximal / próximos passos
- Confirmar que o quiz da aula foi respondido sem espiar (recall).
- Fatia 2: aprofundar `HasConversion` + jsonb + por que o ValueComparer é necessário.
- Ainda não vimos: migration real, DI do DbContext, conexão. Vem nas fatias 3–4.

## Pontos a revisitar
- `Usuario.IsAdmin` (propriedade C#) default `true` diverge do default da coluna (`false`).
  Não é bug agora (seed cria admins), mas anotar quando mexer no cadastro de usuário.
