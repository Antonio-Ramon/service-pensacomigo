# 02 — Auth Google + JWT + perfil

**What to build:** Autor loga com Google (frontend faz o OAuth e manda o token), o backend valida a assinatura, acha o usuário do seed por email, recusa qualquer email fora do seed, emite JWT próprio e devolve o perfil. Requisições seguintes autenticam com esse JWT.

**Blocked by:** 01 — Fundação.

**Status:** done (código; pendências de infra abaixo)

- [x] Login: valida token Google, localiza usuário por email; email fora do seed → recusado (não cria usuário)
- [x] Primeiro login preenche `google_id` e foto a partir da conta Google
- [x] Emite JWT próprio (claims mínimas: id, email, is_admin)
- [x] `GET` perfil autenticado retorna nome e foto
- [ ] Teste de integração: login válido emite JWT; email fora do seed é recusado — *não rodou aqui (sem Docker); precisa também de token Google real ou fake do seam*

**Feito:** `LoginGoogleCommand/Handler/Validator` + `AuthController POST login`; seams `IGoogleTokenValidator`/`IJwtTokenGenerator` com impls `GoogleTokenValidator` (Google.Apis.Auth, valida assinatura + `aud==Google:ClientId`) e `JwtTokenGenerator` (chave simétrica `Jwt:Key`, claims sub/email/is_admin, 8h). `ObterPerfilQuery/Handler` + `UsuariosController [Authorize] GET me` (lê `sub` do `ClaimsPrincipal`, `MapInboundClaims=false`). DI dos seams no `Program`.

**Pendências de infra:** `Google:ClientId` e `Jwt:Key` reais via user-secrets; teste de integração do login precisa de Docker + estratégia pra token Google (fake do `IGoogleTokenValidator`).
