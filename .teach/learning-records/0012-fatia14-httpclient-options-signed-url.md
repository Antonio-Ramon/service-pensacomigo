# 0012 — Fatia 14: typed HttpClient + Options pattern (signed URL de imagem)

**Data:** 2026-08-03 · **Aula:** `lessons/0014-httpclient-options-signed-url.html` · **Issue:** 04 — Imagens (signed URL)

## Contexto
Primeira chamada HTTP de saída do projeto. O backend não recebe o binário: autoriza o autor,
pede uma signed URL ao Supabase Storage e devolve. O front dá `PUT` direto lá (Decisão #14).

## O que foi aprendido
- **`IHttpClientFactory` / typed client**: `new HttpClient()` por chamada esgota socket
  (`TIME_WAIT`); `static HttpClient` eterno congela o DNS. `AddHttpClient<IStorage, SupabaseStorage>`
  resolve os dois — o `HttpMessageHandler` fica num pool reciclado (~2 min).
- **`AddHttpClient<TInterface, TImpl>` já registra a implementação** — não precisa (nem deve)
  somar um `AddScoped` do mesmo par.
- **`BaseAddress` precisa de barra no fim** e o caminho relativo *não* pode começar com `/`,
  senão o `/storage/v1` é substituído em vez de concatenado.
- **Options pattern**: `IOptions<SupabaseOptions>` no lugar de `config["Supabase:Url"]`.
  O ganho real é `ValidateDataAnnotations().ValidateOnStart()` — config faltando derruba a
  subida, não o primeiro upload.
- **`ConfigureTestServices` roda depois do `Program.cs`** → é o gancho pra trocar um seam por
  fake no teste sem tocar na `PensaComigoApiFactory`.

## Decisões
- **Seam `IStorage` na Application, impl no Web** — mesma regra da Fatia 10 (auth). O `Shared`
  citado na arquitetura §Estrutura continua vazio; não vale criar camada pra uma classe.
- **O `path` é montado no servidor** (`posts/{usuarioId-da-claim}/{guid}{ext}`), o cliente só
  manda o nome do arquivo e influencia apenas a extensão (whitelist no validator). Uma URL
  assinada é permissão de escrita: path vindo do cliente = escrever na pasta de outro autor.
  **Desvio consciente** do texto da issue (`GerarUrlUploadAssinada(path)`).
- **`IQuery`, não `ICommand`** — não grava nada no Postgres, então nada pro `UnitOfWorkBehavior`
  commitar. Regra literal do nosso CQRS: mudou linha? Command. Não mudou? Query.
- **Falha do Supabase → `RegraDeNegocioException` (422)**, reusando o handler global. Mesmo
  atalho da Fatia 10: ainda não existe tipo de exceção pra "dependência externa fora do ar" (502).

## A revisitar
- **502 vs 422** quando o Supabase cair: hoje o autor vê 422 (erro dele) por um problema que
  não é dele. Vale um `DependenciaExternaException` quando houver a segunda integração.
- **Retry/timeout**: o typed client está sem política. `AddStandardResilienceHandler`
  (Microsoft.Extensions.Http.Resilience) é uma linha — adicionar quando/se houver flakiness real.
- **Expiração da URL** não é configurável no endpoint; hoje é o padrão do Supabase (2h).
- Faltou rodar os testes: **Docker ausente nesta máquina** (mesma pendência das Fatias 8–13).
