# 0005 — Fatia 6: ExceptionHandling + exceções tipadas

**Data:** 2026-07-18
**Aula:** lessons/0006-exception-handling.html

## O que foi coberto
- Padrão: Application/Domain só **lança** exceção com significado; a Web tem **um** tradutor
  exceção-tipada → status HTTP. Zero `try/catch` por controller, zero HTTP no domínio.
- Exceção tipada = o **tipo** carrega a informação (`NaoEncontradoException` → 404), não a
  string. O `switch` por padrão de tipo (pattern matching) casa tipo → status.
- `IExceptionHandler` (nativo desde .NET 8) substitui middleware manual. Implementa
  `TryHandleAsync`; registra com `AddExceptionHandler<T>()` + `app.UseExceptionHandler()`.
- **ProblemDetails** (RFC 7807): corpo de erro padronizado (status/title/detail) via
  `IProblemDetailsService`. Erros de validação empilhados campo-a-campo em `Extensions["erros"]`.
- Só o `_` (500) é logado — é bug nosso; 404/422 são fluxo esperado.

## Entregue no código
- `Domain/Exceptions/NaoEncontradoException.cs` (404), `RegraDeNegocioException.cs` (422).
- `Web/Exceptions/GlobalExceptionHandler.cs` — `IExceptionHandler`, switch tipo→status,
  ProblemDetails, `ValidationException` (FluentValidation) → 422 com erros por campo.
- `Program.cs` — `AddProblemDetails()` + `AddExceptionHandler<GlobalExceptionHandler>()` +
  `app.UseExceptionHandler()` (bem no topo do pipe).
- Build verde (8 projetos, 0 erros).

## Decisões / ponytail
- Usado `IExceptionHandler` nativo em vez do "ExceptionHandlingMiddleware" que o ticket citava:
  mesmo efeito, menos código, integra com ProblemDetails. Reutilizar plataforma > reinventar.
- **429 (rate limit) ficou de fora**: não há limitador ainda. Entra quando existir (não YAGNI agora).
- Sem controller/Command real → não dá pra exercitar de ponta a ponta ainda. Validação real
  do fluxo vem na Fatia 8 (harness WebApplicationFactory + Testcontainers).

## Próximos passos
- Confirmar checkpoint da aula 06 (4 perguntas) sem espiar.
- Fatia 7: Swagger/OpenAPI + versionamento `/api/v1` + JwtBearer.
