# Missão

Aprender **C# / .NET 10** e as arquiteturas que o projeto Pensa Comigo usa
(**EF Core, MediatR, CQRS, FluentValidation, Clean Architecture**) na prática —
implementando o backend real, fatia por fatia.

## Contexto do aprendiz
- Programa bem em **outra linguagem**; C# é sintaxe/idioms novos.
- Quer entender **antes** de seguir: cada conceito vira uma aula curta + implementação junto.

## Formato acordado
- Fatiar o trabalho por **conceito** (não por arquivo).
- Cada fatia: 1 aula curta → implementamos no código real → checkpoint de entendimento.
- Idioma: **pt-br**. Comunicação concisa.

## Veículo de aprendizado
O backend `service-pensacomigo` (Clean Architecture, 5 projetos). Começando pelo
**Ticket 01 — Fundação (Persistence + Spine Web/Application)**.

## Roadmap do Ticket 01 (fatias)
1. **Mapeamento EF Core + DbContext** ← *estamos aqui*
2. Conversor `jsonb` para `List<Bloco>` (HasConversion)
3. Repository pattern (interfaces no Domain, esqueletos no Persistence, DI)
4. Migration inicial + seed + conexão Supabase
5. Pipeline MediatR (CQRS + Validation/Logging/UnitOfWork behaviors)
6. ExceptionHandlingMiddleware + exceções tipadas
7. Swagger + versionamento + JWT
8. Harness de integração (WebApplicationFactory + Testcontainers)
