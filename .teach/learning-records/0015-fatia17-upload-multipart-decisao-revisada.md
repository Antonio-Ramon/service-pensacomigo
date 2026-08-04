# 0015 — Fatia 17: upload multipart pelo backend (Decisão #14 revisada)

**Data:** 2026-08-03 · **Aula:** `lessons/0017-upload-multipart-stream.html` · **Issue:** 04 — Imagens

## Contexto
A pergunta foi "como eu faço upload com o endpoint de imagens?". A resposta (pede signed URL →
`PUT` direto no Supabase) não convenceu: o usuário quis o arquivo passando pelo backend. Decisão de
arquitetura registrada (#14) foi **revisada**, não contornada.

## O que foi aprendido
- **`IFormFile` é tipo do ASP.NET e para no controller.** O Command recebe nome, tamanho e
  `Stream`. Terceira aplicação da mesma regra (Fatias 10, 14, 17): a Application não conhece o host.
- **`Stream` > `byte[]`** em upload: `byte[]` materializa tudo no heap e cai no Large Object Heap
  (>85 KB), que fragmenta. Com stream a memória fica constante.
- `[Consumes("multipart/form-data")]` (model binder + Swagger) e `[RequestSizeLimit]` (corta no
  servidor) resolvem coisas diferentes — o validator continua existindo pela mensagem amigável.
- **Nome do parâmetro = nome do campo do multipart.** `IFormFile arquivo` ↔ parte `arquivo`.
- `MultipartFormDataContent { { conteudo, "campo", "nome.png" } }` monta o corpo no teste.
- **Fake que registra > fake que devolve**: `StorageFake.UltimoContentType` é o que permite provar
  que o content-type veio da whitelist e não do cliente.

## Decisões
- **Decisão #14 revisada** na tabela de arquitetura (com data e motivo, linha antiga tachada, não
  apagada): upload pelo backend. Ganha um passo a menos no cliente, validação antes do bucket e o
  fornecedor escondido; paga o binário atravessando o .NET (mitigado: `Stream`, 5 MB, sem `byte[]`).
- **Content-type derivado da extensão**, nunca do multipart — `.png` gravado como `text/html` e
  servido pelo storage é XSS. `ImagensPermitidas` é a whitelist única (tipos + tamanho), consumida
  pelo validator, pelo handler e pelo atributo do controller.
- **`IQuery` → `ICommand`**: o critério passou a ser *efeito colateral no mundo*, não *escrita no
  nosso Postgres*. `SaveChanges` sem mudanças é no-op, custo zero.
- A rota virou `POST /api/v1/imagens` (era `/imagens/url-upload`); o caso de uso antigo foi
  **deletado**, não mantido em paralelo.

## A revisitar
- **Ninguém confere que os bytes são imagem** — só extensão e tamanho. Magic number nos primeiros
  bytes se virar problema.
- **`POST object/{bucket}/{path}` não foi conferido contra o Supabase real** (a rota de signed URL
  tinha sido). É o próximo passo de infra, junto com `dotnet run` + Swagger.
- Imagem órfã (sobe, autor desiste do post) não tem faxina.
- O `imagemCapa` que chega no `CriarPostCommand` continua sendo string livre — o backend agora sabe
  qual path ele mesmo gerou, então validar o prefixo `posts/{claim sub}/` ficou mais fácil de
  justificar. Fica pra fatia de edição.
