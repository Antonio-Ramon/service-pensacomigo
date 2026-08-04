# 04 — Imagens (upload pelo backend)

**What to build:** Autor autenticado envia a imagem para a API (multipart); o backend valida, monta o path e repassa ao Supabase Storage, devolvendo `{ path, url }`. O `path` é o que o autor guarda depois na capa/bloco do post; a `url` é a leitura pública.

**Blocked by:** 02 — Auth.

**Status:** done

- [x] Seam `Application/Storage/IStorage.EnviarAsync(path, conteudo, contentType, ct)` + impl
      `Web/Storage/SupabaseStorage` (typed HttpClient + `IOptions<SupabaseOptions>`)
      — mora no host, não em `Shared`, seguindo o precedente dos seams de auth (Fatia 10).
- [x] Endpoint autenticado recebe multipart: `POST /api/v1/imagens` (campo `arquivo`)
      → `{ path, url }`
- [x] Teste: exige JWT, path na pasta do autor, content-type vindo da whitelist, extensão
      inválida e arquivo vazio → 422 (`ImagensTests`, seam trocado por fake via `ConfigureTestServices`)

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — sem endpoint de listagem aqui.

## Desvios / decisões
- **Decisão #14 revisada (2026-08-03): upload passa pelo backend**, não mais signed URL com o
  frontend subindo direto. Motivo: um passo em vez de dois para o cliente; validação real
  (tamanho, extensão, content-type) *antes* de qualquer byte chegar ao bucket; chave e fornecedor
  escondidos atrás da nossa API. Custo aceito: o binário atravessa o .NET — mitigado com
  `Stream` de ponta a ponta (sem `byte[]`), limite de 5 MB no `[RequestSizeLimit]` e no validator.
- **O `path` é montado no servidor** (`posts/{usuarioId-da-claim}/{guid}{ext}`), nunca recebido do
  cliente. Do nome enviado só a extensão sobrevive (whitelist `.jpg/.jpeg/.png/.webp`).
- **O content-type sai da whitelist**, não do que o cliente declarou no multipart: um `.png`
  servido pelo storage com `Content-Type: text/html` viraria XSS.
- Caso de uso é `ICommand` (era `IQuery` na versão signed URL): agora tem efeito colateral de
  verdade. Nada muda no Postgres, então o `UnitOfWorkBehavior` commita um conjunto vazio — no-op.

## Follow-ups
- [x] `Supabase:Url` e `Supabase:ServiceRoleKey` reais via user-secrets + bucket `imagens`
      criado no projeto Supabase (público p/ leitura, limite 5 MB, MIME jpeg/png/webp).
      **Nome do bucket é case-sensitive** — criado como `Imagens` dava 404.
- [ ] Conferir o `POST object/{bucket}/{path}` contra o Supabase real (a versão signed URL já foi
      conferida; esta rota ainda não).
- [ ] Teste de integração não rodou aqui — **Docker ausente na máquina** (mesma pendência das
      issues anteriores).
- [ ] Nada valida que os bytes são mesmo uma imagem (só extensão + tamanho). Se virar problema,
      checar magic number nos primeiros bytes do stream.
- [ ] Falha do Supabase hoje vira 422 (`RegraDeNegocioException`). Quando houver a segunda
      integração externa, criar um tipo próprio → 502.
- [ ] Sem retry/timeout no typed client. `AddStandardResilienceHandler` se aparecer flakiness.
- [ ] Imagem órfã: arquivo sobe, autor desiste do post. Faxina periódica se o bucket encher.
