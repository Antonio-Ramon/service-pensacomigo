# 02 — Auth Google + JWT + perfil

**What to build:** Autor loga com Google (frontend faz o OAuth e manda o token), o backend valida a assinatura, acha o usuário do seed por email, recusa qualquer email fora do seed, emite JWT próprio e devolve o perfil. Requisições seguintes autenticam com esse JWT.

**Blocked by:** 01 — Fundação.

**Status:** ready-for-agent

- [ ] Login: valida token Google, localiza usuário por email; email fora do seed → recusado (não cria usuário)
- [ ] Primeiro login preenche `google_id` e foto a partir da conta Google
- [ ] Emite JWT próprio (claims mínimas: id, email, is_admin)
- [ ] `GET` perfil autenticado retorna nome e foto
- [ ] Teste de integração: login válido emite JWT; email fora do seed é recusado
