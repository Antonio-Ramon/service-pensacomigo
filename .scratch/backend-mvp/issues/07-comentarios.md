# 07 — Comentários

**What to build:** Leitor comenta num post informando o nome (sem conta) e pode responder a um comentário (1 nível só). Moderação automática estilo YouTube: rate limit por visitante e filtro de palavrão — comentário limpo publica na hora. Leitor lista os comentários aprovados com respostas. Admin autenticado esconde ou deleta comentário publicado.

**Blocked by:** 05 — Post CRUD (post precisa existir), 02 — Auth (moderação exige admin).

**Status:** done (código completo; testes de integração pendentes de execução — sem Docker na máquina)

- [x] Comentar com nome (anônimo); resposta 1 nível; 2º nível bloqueado no handler/validator (não no schema)
- [x] Rate limit: 5 comentários por `viewer_hash` em janela deslizante de 1 min → 429; estado em cache de memória (`ponytail:` Redis) [unit: avaliação do rate limit]
- [x] Filtro de palavrão ~~(lista no Shared)~~ **em `Application/Common`**; match → erro de validação, nada sujo entra no banco [unit] — *Shared referencia Application, então o validator não enxergaria de volta (mesmo motivo do `GeradorSlug`)*
- [x] Comentário limpo publica imediatamente (`aprovado = true`)
- [x] Listar comentários aprovados de um post, com respostas (anônimo), via **Gridify**: `ListarComentariosQuery : GridifyQuery` (filtra sempre por `post_id` + `aprovado`), paginação/ordenação da querystring, resposta no envelope `Pagina<T>`. Ver arquitetura §7.1 / Decisão #19 — *pagina só as RAÍZES; respostas vêm por filtered include*
- [x] Admin (autenticado) esconde (`aprovado=false`) ou deleta comentário — *policy `Admin` sobre a claim `is_admin`*
- [x] Teste de integração: fluxo comentar/responder; palavrão bloqueado; 6º comentário no minuto → 429 — *escritos, não executados aqui (sem Docker); moderação vem na próxima fatia*

**Feito (escrita):** `CriarComentarioCommand/Handler/Validator` + `ComentariosController` em rota aninhada
`POST api/v1/posts/{postId:guid}/comentarios` (anônimo). Funções puras `FiltroPalavrao` (reusa
`GeradorSlug.Gerar` pra normalizar e compara palavra inteira) e `JanelaDeslizante.Registrar`
(carimbos + relógio como parâmetro → unit test sem esperar o minuto passar), ambas em
`Application/Common`. `LimitadorDeComentarios` (singleton + `IMemoryCache`) é a única cola com
estado. `MuitasRequisicoesException` → 429 no `GlobalExceptionHandler` (o status que ficou de fora
da fatia 6). `IPostRepository.ExistePorIdAsync` (vira `EXISTS`, não carrega a entidade).

**Feito (leitura + moderação):** `GET api/v1/posts/{postId}/comentarios` (anônimo) pagina as **raízes**
aprovadas e traz as respostas aprovadas por *filtered include*; `PATCH {id}/ocultar` e `DELETE {id}`
sob `[Authorize(Policy = "Admin")]`.

**Decisões:**
- **O `GridifyMapper` é fronteira de segurança.** `aprovado`/`postId`/`parentId` ficam fora dele:
  mapeados, `?filter=aprovado=false` viraria painel público do que a moderação escondeu. O recorte
  fixo mora no `Where` do repo; a querystring só escolhe dentro dele. O `postId` do request é
  sobrescrito pelo valor da rota depois do binding.
- **Paginar as raízes, não as linhas.** `pageSize` conta conversas — paginar a tabela plana cortaria
  uma conversa no meio e deixaria resposta sem o pai visível.
- **Autorização por claim, não por token.** Policy `Admin` (`RequireClaim("is_admin", "true")`) num
  lugar só. `RequireClaim` compara string exata e `bool.ToString()` dá `"True"` → o
  `JwtTokenGenerator` passou a emitir `"true"` minúsculo.
- **Ocultar preserva, deletar não.** Ocultar é reversível e mantém o texto para auditoria; deletar é
  físico e leva as respostas pela cascata do `parent_id`. Nenhum dos dois toca nas respostas
  explicitamente — a listagem só devolve resposta de raiz visível.
- **`viewer_hash` é calculado no servidor** (`Web/Visitantes/HashVisitante`): SHA-256 de
  IP + User-Agent. Se viesse do cliente, trocar o valor zeraria o limite — e, nos likes (issue 08),
  permitiria curtir infinitas vezes. O hash também evita guardar IP cru (dado pessoal).
- **Palavrão é validação, 1 nível é handler.** O filtro roda no `ValidationBehavior` (falha de
  formato do texto, 422 campo-a-campo); "resposta de resposta" é invariante de negócio, precisa
  ler o pai no banco → `RegraDeNegocioException` no handler.
- `ComentarioResponse` **não traz `DataCriacao`**: a coluna tem `default now()` e o commit é do
  `UnitOfWorkBehavior`, depois do handler — o valor ainda seria `default(DateTime)`. A data sai na
  listagem, que lê do banco.
- Cada teste de integração usa um **User-Agent próprio**: com um só, o 6º comentário do arquivo
  inteiro tomaria 429 e os testes se contaminariam.
