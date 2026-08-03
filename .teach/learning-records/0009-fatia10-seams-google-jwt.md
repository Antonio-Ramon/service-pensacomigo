# 0009 — Fatia 10: Preenchendo os seams (Google + JWT)

**Data:** 2026-08-01
**Aula:** lessons/0010-seams-google-jwt.html
**Ticket:** 02 — Auth Google + JWT

## O que foi coberto
- **Onde a impl de um seam mora**: interface na Application; impl no host **Web** (`Web/Auth/`),
  porque usa lib externa que a Application não pode conhecer. Seta de dependência: fora → dentro.
- **Inversão de dependência na prática**: handler manda no contrato, host escolhe o concreto via DI.
- **Validar token Google** (`GoogleJsonWebSignature.ValidateAsync`): confere a assinatura contra as
  chaves públicas do Google + exige `aud == Google:ClientId`. `InvalidJwtException` → `RegraDeNegocioException` (422).
- **Emitir JWT próprio**: mesma chave/issuer/audience que o `Program.cs` valida (Fatia 7).
  Claims = afirmações assinadas (`sub`, `email`, `is_admin`); expira em 8h.
- **Dois tokens distintos**: idToken Google prova identidade UMA vez no login; JWT próprio = a sessão depois.
- **Registro na DI**: 2 linhas `AddScoped` ligam interface→impl; sem elas o handler não monta.

## Entregue no código
- `Web/Auth/GoogleTokenValidator.cs`, `Web/Auth/JwtTokenGenerator.cs`.
- `Program.cs`: usings + 2 `AddScoped` dos seams.
- `appsettings.json`: seção `Google:ClientId` (valor real via user-secrets).
- Pacote `Google.Apis.Auth` 1.75.0 no Web.
- Build verde (8 projetos, 0 erros, 0 warnings).

## Decisões / ponytail
- **Sem projeto Infrastructure separado**: com um host só, Web é a composition root natural (YAGNI).
- **Token Google inválido → 422** (reusa `RegraDeNegocioException`). 401 seria mais correto, mas
  não há tipo de exceção p/ 401 ainda. Marcado com `ponytail:` no validator.
- JWT gerado com `System.IdentityModel.Tokens.Jwt` (transitivo via JwtBearer) — sem pacote novo p/ isso.

## Pendências de infra (não-código)
- `Google:ClientId` e `Jwt:Key` reais via user-secrets p/ rodar de verdade.
- Teste de integração do login (fake do IGoogleTokenValidator) depende de Docker — **não roda nesta máquina**.

## Próximos passos
- Confirmar checkpoint da aula 10 (3 perguntas) sem espiar.
- **Fatia 11** — `GET` perfil autenticado: `[Authorize]` + ler as claims do `User` (`ClaimsPrincipal`).
