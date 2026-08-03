# 04 — Imagens (signed URL)

**What to build:** Autor autenticado pede uma signed URL de upload e envia a imagem direto ao Supabase Storage — o binário não passa pelo backend. O path retornado é o que o autor guarda depois na capa/bloco do post.

**Blocked by:** 02 — Auth.

**Status:** done

- [x] Seam `Application/Storage/IStorage.GerarUrlUploadAssinadaAsync(path, ct)` + impl
      `Web/Storage/SupabaseStorage` (typed HttpClient + `IOptions<SupabaseOptions>`)
      — mora no host, não em `Shared`, seguindo o precedente dos seams de auth (Fatia 10).
- [x] Endpoint fino (autenticado) devolve a signed URL do Supabase Storage:
      `POST /api/v1/imagens/url-upload` → `{ path, urlAssinada }`
- [x] Teste: endpoint exige JWT, devolve URL assinada e recusa extensão fora da whitelist
      (`ImagensTests`, seam trocado por fake via `ConfigureTestServices`)

> Gridify (Decisão #19 / arquitetura §7.1): **não se aplica** — sem endpoint de listagem aqui.

## Desvios / decisões
- **O `path` é montado no servidor** (`posts/{usuarioId-da-claim}/{guid}{ext}`), não recebido do
  cliente como o texto original sugeria. Signed URL é permissão de escrita — path do cliente
  permitiria subir na pasta de outro autor. O corpo só manda `nomeArquivo`, e dele só a extensão
  sobrevive (whitelist `.jpg/.jpeg/.png/.webp`).
- Caso de uso é `IQuery`, não `ICommand`: nada é gravado no Postgres.

## Follow-ups
- [x] `Supabase:Url` e `Supabase:ServiceRoleKey` reais via user-secrets + bucket `imagens`
      criado no projeto Supabase (público p/ leitura, limite 5 MB, MIME jpeg/png/webp).
      Conferido: `POST object/upload/sign/imagens/…` devolve `{ url, token }` (token vale 2 h)
      e a API sobe com o `ValidateOnStart` passando. **Nome do bucket é case-sensitive** —
      criado como `Imagens` dava 404 `The related resource does not exist`.
- [ ] Teste de integração não rodou aqui — **Docker ausente na máquina** (mesma pendência das
      issues anteriores).
- [ ] Falha do Supabase hoje vira 422 (`RegraDeNegocioException`). Quando houver a segunda
      integração externa, criar um tipo próprio → 502.
- [ ] Sem retry/timeout no typed client. `AddStandardResilienceHandler` se aparecer flakiness.
