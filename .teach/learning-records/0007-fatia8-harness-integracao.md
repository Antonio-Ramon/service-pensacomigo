# 0007 — Fatia 8: Harness de integração

**Data:** 2026-07-18
**Aula:** lessons/0008-harness-integracao.html

## O que foi coberto
- Unit (classe isolada, sem banco) vs **integração** (sobe a app inteira: DI, pipeline MediatR,
  EF Core, migration, banco real). Negócio se testa por integração; nada de mockar EF.
- `WebApplicationFactory<Program>` (pacote `Microsoft.AspNetCore.Mvc.Testing`): instancia o
  `Program.cs` em memória, dá `HttpClient` + acesso à mesma DI de produção.
- `public partial class Program;` no fim do `Program.cs`: top-level statements geram `Program`
  internal; o partial o torna público para o teste referenciar o tipo.
- **Testcontainers**: Postgres real e descartável em Docker. Mesmo provider Npgsql, mesmo `jsonb`,
  mesma migration de produção — SQLite em memória divergiria.
- Costura: `ConfigureWebHost` sobrescreve só `ConnectionStrings:Default` (DI intacta, pois
  `AddPersistence` lê a string só na construção do DbContext). `IAsyncLifetime` do xunit sobe o
  container + aplica migration antes dos testes e descarta depois.
- `IClassFixture<Factory>`: uma fábrica (um container) compartilhada pela classe de teste.

## Entregue no código
- `Program.cs` — `public partial class Program;` no fim.
- `PensaComigo.IntegrationTests.csproj` — `Microsoft.AspNetCore.Mvc.Testing` 10.0.9 +
  `Testcontainers.PostgreSql` 4.6.0.
- `PensaComigoApiFactory.cs` — WebApplicationFactory + PostgreSqlContainer; migration aplicada no start.
- `SpineSmokeTests.cs` — 1 teste: migration aplica e seed (Antonio/Jessica) presente.
- Build verde (6 projetos, 0 erros).

## Decisões / ponytail
- **Override mínimo**: só a connection string via `UseSetting`, não re-registro de DbContext.
  `AddPersistence` lê a string tarde o bastante para o desvio funcionar.
- Smoke test bate no DbContext pela DI da fábrica (não há controller ainda p/ HTTP end-to-end);
  quando a Fatia 02 trouxer endpoint, o `HttpClient` da fábrica entra.

## Observações técnicas encontradas
- **Choque de `DisposeAsync`**: xunit v2 `IAsyncLifetime` retorna `Task`; base
  `WebApplicationFactory.DisposeAsync` retorna `ValueTask`. Erro CS0738. Resolvido implementando
  `IAsyncLifetime` **explicitamente** (`async Task IAsyncLifetime.InitializeAsync/DisposeAsync`).
- Warning MSB3277 benigno: EF Core Relational 10.0.4 (transitivo via Mvc.Testing/Testcontainers)
  vs 10.0.9 do Persistence. Unificação escolhe uma; sem impacto observado.

## Pré-requisito de infra (do usuário)
- Testcontainers exige **Docker rodando**. Nesta máquina o `docker` não está no PATH → o teste
  não executou aqui. Rodar `dotnet test` com Docker Desktop no ar para ver verde.

## Próximos passos
- Confirmar checkpoint da aula 08 (4 perguntas) sem espiar.
- Subir Docker + `dotnet test` → smoke verde fecha o Ticket 01.
- Iniciar **Ticket 02 — Auth Google + JWT** (primeiro caso de uso real; primeiro controller).
