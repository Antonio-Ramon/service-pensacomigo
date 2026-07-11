# 0002 — Fatia 3: Repository pattern + DI

**Data:** 2026-07-11
**Aula:** lessons/0003-repository-pattern.html

## O que foi coberto
- Inversão de dependência: interface no Domain, implementação no Persistence; a
  seta de dependência do Persistence sobe pro Domain (Clean Architecture).
- Anatomia da interface: `Task<T?>`, `CancellationToken ct = default`.
- Implementação com primary constructor (C# 12) recebendo o DbContext.
- Repositório rastreia (`AddAsync`), **não** dá `SaveChanges` — isso é do
  UnitOfWorkBehavior (Fatia 5). Seam explícito.
- DI: `AddScoped<Interface, Impl>`, tempo de vida por request, extension method
  `AddPersistence()` chamada no Program.cs.

## Entregue no código
- Domain/Repositories: `IPostRepository`, `IComentarioRepository`,
  `IUsuarioRepository`, `ITagRepository` (2 métodos cada: ObterPorId, Adicionar).
- Persistence/Repositories: 4 implementações esqueleto.
- Persistence/DependencyInjection.cs `AddPersistence()` registrando os 4 repos.
- Program.cs chama `AddPersistence()`. Build verde.

## Decisões / ponytail
- Métodos mínimos ("crescem por fatia", conforme ticket) — só ObterPorId + Adicionar.
- **DbContext ainda não registrado na DI** (falta connection string Supabase).
  Fica na Fatia 4. Marcado com comentário ponytail em DependencyInjection.cs.
  Nada resolve os repos ainda (sem controllers), então build/uso não quebra.

## Próximos passos
- Confirmar checkpoint da aula (3 perguntas) sem espiar.
- Fatia 4: migration inicial + seed + AddDbContext com conexão Supabase (SSL,
  direta 5432 p/ migration, pooled 6543 p/ runtime).
