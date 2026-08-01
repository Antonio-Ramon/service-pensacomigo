# 0008 — Fatia 9: A fatia vertical CQRS (login)

**Data:** 2026-08-01
**Aula:** lessons/0009-caso-de-uso-vertical-cqrs.html
**Ticket:** 02 — Auth Google + JWT (aberto)

## O que foi coberto
- Primeira fatia vertical: Controller → `mediator.Send` → cebola de behaviors → Handler → Response.
- **Command** = `record` mensagem (`LoginGoogleCommand(string IdToken) : ICommand<LoginResponse>`).
  Command (não Query) porque o 1º login grava google_id/foto → passa pelo UnitOfWork e commita.
- **Handler** = onde a regra vive; um por command; colaboradores por DI construtor.
- **Controller magro**: `[HttpPost("login")]` só faz `Ok(await mediator.Send(command, ct))`. ASP.NET
  desserializa o corpo direto no command. Zero regra no controller.
- **Primeiro validator real** (`LoginGoogleCommandValidator : AbstractValidator<>`): achado sozinho
  pelo `AddValidatorsFromAssembly`, rodado pelo `ValidationBehavior` antes do handler. Não se registra à mão.
- **Seams**: `IGoogleTokenValidator` + `IJwtTokenGenerator` — interfaces sem impl ainda. Motivo duplo:
  regra da Clean Arch (Application não conhece lib do Google) + testabilidade (fake no teste de integração).

## Entregue no código
- `Application/Auth/IGoogleTokenValidator.cs` (+ record `GoogleUserInfo`), `IJwtTokenGenerator.cs`.
- `Application/Auth/Login/`: `LoginGoogleCommand`, `LoginResponse`, `LoginGoogleCommandValidator`,
  `LoginGoogleCommandHandler`.
- `Domain/Repositories/IUsuarioRepository` + `Persistence/.../UsuarioRepository`: `ObterPorEmailAsync`.
- `Web/Controllers/AuthController.cs` — POST `api/v1/auth/login`.
- Build verde (6 projetos, 0 erros, 1 warning pré-existente MSB3277).

## Decisões / ponytail
- `LoginResponse` mora na **Application** (não no Shared): Shared referencia Application → poria a
  resposta em ciclo. Resposta de MediatR fica junto do handler.
- Email fora do seed → `NaoEncontradoException` (404). Se quiser 401/403 explícito depois, criar
  exceção tipada nova. Marcado com `ponytail:` no handler.
- **Endpoint ainda não roda ponta a ponta**: faltam as impls dos 2 seams + registro na DI. É a Fatia 10.

## Próximos passos
- Confirmar checkpoint da aula 09 (3 perguntas) sem espiar.
- **Fatia 10** — implementar os seams: `GoogleTokenValidator` (Google.Apis.Auth) +
  `JwtTokenGenerator` (chave simétrica `Jwt:Key`, claims id/email/is_admin) + registro na DI.
  Aí o login roda de verdade e cabe o teste de integração (válido emite JWT; fora do seed recusa).
- **Fatia 11** — `GET` perfil autenticado: `[Authorize]` + ler claims do `ClaimsPrincipal`.
