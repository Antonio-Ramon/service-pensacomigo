# 04 — Imagens (signed URL)

**What to build:** Autor autenticado pede uma signed URL de upload e envia a imagem direto ao Supabase Storage — o binário não passa pelo backend. O path retornado é o que o autor guarda depois na capa/bloco do post.

**Blocked by:** 02 — Auth.

**Status:** ready-for-agent

- [ ] `Shared/Storage.GerarUrlUploadAssinada(path)`
- [ ] Endpoint fino (autenticado) devolve a signed URL do Supabase Storage
- [ ] Teste: endpoint exige JWT e devolve URL assinada para um path

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — sem endpoint de listagem aqui.
