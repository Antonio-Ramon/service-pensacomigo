# 0006 — Fatia 7: Swagger + JWT + versionamento

**Data:** 2026-07-18
**Aula:** lessons/0007-swagger-jwt-versionamento.html

## O que foi coberto
- Dois JWTs no fluxo: token do Google (validado 1x no login, Fatia 02) vs **JWT próprio**
  do backend (validado a cada request). Esta fatia só configura o segundo.
- Chave **simétrica** (`SymmetricSecurityKey`): quem assina e valida é a mesma parte (nós).
  Segredo via user-secrets (`Jwt:Key`), nunca no git.
- `TokenValidationParameters`: cada `Validate*` fecha uma porta (Issuer/Audience/Lifetime/
  SigningKey). Segurança = exceção à regra do código mínimo; ligar tudo.
- Ordem no pipe: `UseAuthentication` (identidade, preenche `HttpContext.User`) ANTES de
  `UseAuthorization` (permissão, checa `[Authorize]`). Inverter quebra toda rota protegida.
- Swagger Authorize: `AddSecurityDefinition` (descreve esquema Bearer) + `AddSecurityRequirement`
  (aplica a todos). Cadeado no topo do Swagger manda o token nas chamadas.

## Entregue no código
- `Program.cs` — `AddAuthentication(JwtBearer).AddJwtBearer(...)` com todos os `Validate*`,
  `AddAuthorization()`, `UseAuthentication()` antes de `UseAuthorization()`. SwaggerGen com
  security definition + requirement Bearer.
- `appsettings.json` — seção `Jwt` (Issuer/Audience/Key vazia; Key real via user-secrets).
- Build + boot verdes ("Now listening").

## Decisões / ponytail
- **Versionamento sem lib**: `/api/v1` vira `[Route("api/v1/[controller]")]` quando o primeiro
  controller chegar. `Asp.Versioning` só quando existir v2 — máquina de versão pra uma versão é peso morto.
- Chave `Jwt:Key` vazia no boot NÃO estoura (validação só é exercida quando um token chega).
  Real key via user-secrets antes de rodar auth de verdade.

## Observações técnicas encontradas
- Swashbuckle 10.2.3 puxa Microsoft.OpenApi **2.x** → churn de API resolvido no caminho:
  namespace `Microsoft.OpenApi` (não `.Models`); referência é `OpenApiSecuritySchemeReference`
  (não `OpenApiReference`); `AddSecurityRequirement` agora recebe `Func<OpenApiDocument, ...>`.

## Próximos passos
- Confirmar checkpoint da aula 07 (4 perguntas) sem espiar.
- Fatia 8: `public partial class Program {}` + harness de integração (WebApplicationFactory +
  Testcontainers Postgres). Última fatia do Ticket 01.
