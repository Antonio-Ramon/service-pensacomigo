# 0004 — Fatia 5: Pipeline MediatR (behaviors + CQRS)

**Data:** 2026-07-12
**Aula:** lessons/0005-pipeline-mediatr.html

## O que foi coberto
- Behavior = middleware de mensagem (`IPipelineBehavior<TReq,TResp>`). Cebola: cada
  behavior chama `next(ct)` pra ir pra dentro ou corta o fluxo.
- Genérico aberto (`<,>`) → um behavior serve toda mensagem. Registro via
  `AddOpenBehavior(typeof(X<,>))`. **Ordem de registro = ordem de execução** (externo primeiro).
- CQRS: Command escreve, Query lê. Marcador `IBaseCommand` (não-genérico) deixa o
  UnitOfWorkBehavior testar `request is IBaseCommand` e commitar só em Commands.
- Ponte Unit of Work: `IUnitOfWork` no Domain, DbContext implementa (`CommitAsync => SaveChangesAsync`).
  DI aponta `IUnitOfWork` pra MESMA instância Scoped do DbContext — senão rastreio e commit
  cairiam em contextos diferentes e nada salvaria.

## Entregue no código
- `Domain/Repositories/IUnitOfWork.cs` — abstração do commit.
- `PensaComigoDbContext : DbContext, IUnitOfWork` — `CommitAsync` = `SaveChangesAsync`.
- `Application/Messaging/ICommand.cs` — `IBaseCommand`, `ICommand<T>`, `IQuery<T>`.
- `Application/Behaviors/` — Logging, Validation (FluentValidation → `ValidationException`), UnitOfWork.
- `Application/DependencyInjection.AddApplication()` — MediatR + 3 open behaviors (ordem
  Logging→Validation→UnitOfWork) + `AddValidatorsFromAssembly`.
- `Persistence/DependencyInjection` — `AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DbContext>())`.
- `Program.cs` — `AddApplication()` antes de `AddPersistence()`.
- Application csproj ganhou `Microsoft.Extensions.Logging.Abstractions` (pro ILogger).
- Build verde.

## Decisões / ponytail
- `ValidationException` (do FluentValidation) é lançada crua aqui; tradução pra 422 vem no
  ExceptionHandlingMiddleware (Fatia 6). Behavior só sinaliza, não formata resposta HTTP.
- Ainda não há nenhum Command/Query/validator real — os behaviors são o trilho; passam a
  valer quando a primeira fatia de negócio (Ticket 05, posts) chegar.

## Observações técnicas encontradas
- Warning MSB3277 no `PensaComigo.IntegrationTests`: fixa EF Core Relational 10.0.4 vs 10.0.9
  do resto. Pré-existente, não é desta fatia. Resolver na Fatia 8 (harness de integração).

## Próximos passos
- Confirmar checkpoint da aula 05 (3 perguntas) sem espiar.
- Fatia 6: ExceptionHandlingMiddleware + exceções tipadas (404/422/429), controllers magros.
