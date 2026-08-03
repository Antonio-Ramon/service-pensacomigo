# 0010 — Fatia 11: Endpoint protegido + ler claims

**Data:** 2026-08-01
**Aula:** lessons/0011-authorize-claims-perfil.html
**Ticket:** 02 — Auth Google + JWT

## O que foi coberto
- **`[Authorize]`**: atributo na action; o pipeline (UseAuthentication/Authorization da Fatia 7)
  valida o Bearer e corta com **401 automático** sem `if` no código.
- **`User` (`ClaimsPrincipal`)**: preenchido pelo middleware a partir das claims do JWT.
  `User.FindFirstValue(JwtRegisteredClaimNames.Sub)` lê o id que o `JwtTokenGenerator` assinou.
- **Pega-ratão `MapInboundClaims`**: por padrão o handler renomeia `sub`→`nameidentifier`,
  `email`→URL SAML. Ligamos `options.MapInboundClaims = false` no Program p/ nomes intactos.
- **Primeiro Query CQRS**: `ObterPerfilQuery : IQuery<PerfilResponse>` — lado leitura.
  UnitOfWorkBehavior só commita `IBaseCommand`, então Query nunca grava sem querer.
- **Token prova QUEM (offline); nome/foto vêm frescos do banco** no handler.

## Entregue no código
- `Application/Usuarios/Perfil/`: `ObterPerfilQuery`, `ObterPerfilQueryHandler`, `PerfilResponse`.
- `Web/Controllers/UsuariosController.cs`: `[Authorize] GET me`, lê `sub`, `Send(query)`.
- `Program.cs`: `options.MapInboundClaims = false` (refatorou o lambda do AddJwtBearer).
- Build verde (8 projetos, 0 erros). Warning MSB3277 (conflito EF 10.0.4/10.0.9 em IntegrationTests)
  é pré-existente e não relacionado.

## Decisões / ponytail
- Perfil vai por **Query com roundtrip ao banco** (não só claims) porque nome/foto não estão no token.
- Sem validator no Query (id vem de claim já confiável, não de input do usuário).

## Pendências
- Endpoint não roda ponta a ponta aqui: precisa de JWT real (login) + Supabase/Docker.
- Confirmar checkpoint da aula 11 (3 perguntas) sem espiar.

## Próximos passos
- Fechar Ticket 02 (auth completo).
- Fatia 12 — primeiro caso de uso de conteúdo: criar `Post` (Command com a entidade real, blocos jsonb).
