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
1. **Mapeamento EF Core + DbContext**
2. Conversor `jsonb` para `List<Bloco>` (HasConversion)
3. Repository pattern (interfaces no Domain, esqueletos no Persistence, DI)
4. Migration inicial + seed + conexão Supabase
5. Pipeline MediatR (CQRS + Validation/Logging/UnitOfWork behaviors)
6. ExceptionHandlingMiddleware + exceções tipadas
7. Swagger + versionamento + JWT
8. Harness de integração (WebApplicationFactory + Testcontainers) — *Ticket 01 fechado*

## Roadmap do Ticket 02 (Auth Google + JWT)
9. **Fatia vertical CQRS**: 1º Command/Handler/Controller + validator (login) ← *estamos aqui*
10. Implementar os seams: validar token Google (Google.Apis.Auth) + emitir JWT + DI → login roda
11. ✅ `GET` perfil autenticado: `[Authorize]` + ler claims do `ClaimsPrincipal` + 1º Query CQRS

## Roadmap do Ticket 03 (Tags)
12. **Primeira feature CRUD completa**: Command (criar, `[Authorize]`) + Query (listar, `[AllowAnonymous]`)
    no mesmo controller; slug como campo calculado; 422 amigável vs índice único; teste de integração.

## Fatias transversais (decisões de arquitetura que atravessam tickets)
13. ✅ **Gridify — padrão de listagem project-wide**: `GridifyQuery` na Query, `GridifyMapper` como
    whitelist, envelope `Pagina<T>` (`{ items, totalItems }`). Fecha o follow-up da issue 01 e
    conforma a listagem de Tags (issue 03).

## Roadmap do Ticket 04 (Imagens — signed URL)
14. ✅ **Chamar API externa do jeito .NET**: typed `HttpClient` (`AddHttpClient`) + Options pattern
    (`IOptions<T>` + `ValidateOnStart`). Seam `IStorage`, impl Supabase no host, path montado no
    servidor a partir da claim.

## Roadmap do Ticket 05 (Post — criar / editar / deletar)
15. ✅ **Função pura + primeiro teste unitário**: `GeradorSlug` (normaliza + colisão `-N`) e
    `CalculadoraTempoLeitura` em `Application/Common`, testados no projeto `UnitTests`
    (roda sem Docker). Pirâmide de testes, `[Fact]` vs `[Theory]`.
16. ✅ **`CriarPostCommand`**: N:N com Tags via change tracker + `List<Bloco>` no jsonb de verdade;
    `RuleForEach` no modelo flat; autor da claim. ← *estamos aqui*
## Revisão do Ticket 04 (Imagens)
17. ✅ **Upload multipart pelo backend** (Decisão #14 revisada): `IFormFile` → `Stream` na
    Application, whitelist extensão→content-type, e como se reescreve uma decisão de arquitetura.
    ← *estamos aqui*

## Continuação do Ticket 05
18. **Editar (slug congelado) + deletar**: outro lado do change tracker (`Update`/`Remove`) e
    autorização por dono ("é seu mesmo?").
