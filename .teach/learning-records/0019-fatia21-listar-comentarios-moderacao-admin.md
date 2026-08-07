# 0019 — Fatia 21: árvore rasa com filtered include e autorização por claim

**Data:** 2026-08-07 · **Aula:** [0021](../lessons/0021-listar-comentarios-moderacao-admin.html) · **Ticket:** 07 (fechado)

## O que foi aprendido

- **Paginar árvore = paginar as raízes.** A tabela é plana (raiz e resposta são a mesma
  linha, separadas por `parent_id`); paginar linhas cortaria a conversa no meio e deixaria
  resposta órfã. O `Where` do repo recorta `PostId + Aprovado + ParentId == null` e o
  Gridify pagina só isso — `TotalItems` passa a significar "quantas conversas".
- **Filtered include** (`Include(c => c.Respostas.Where(r => r.Aprovado).OrderBy(...))`,
  EF Core 5+): um JOIN com a condição embutida. Alternativas piores: filtrar em memória
  (traz lixo), um SELECT por raiz (N+1), SQL na mão.
- **O `GridifyMapper` é fronteira de segurança, não só conveniência.** `aprovado`, `postId`
  e `parentId` ficaram FORA do mapper de propósito: mapeado, `?filter=aprovado=false`
  viraria painel público de tudo que a moderação escondeu. Recorte fixo mora no repo;
  a querystring só escolhe dentro dele.
- **`[Authorize]` responde "tem token?"; policy responde "quem é você?".**
  `AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireClaim("is_admin", "true")))` +
  `[Authorize(Policy = "Admin")]`. Regra num lugar só; endpoint cita o nome.
- **401 vs 403 sai de graça do middleware.** 401 = não sei quem você é; 403 = sei, e você
  não pode. Nenhum `if` no código decide isso.
- **`RequireClaim` compara string exata** e `bool.ToString()` produz `"True"` com T
  maiúsculo → o `JwtTokenGenerator` passou a emitir `"true"`. Um T a mais e todo admin
  tomaria 403 sem nenhum erro no log. Achado ao ligar a policy, não por teste.
- **Dado que decide o que você enxerga nunca vem do cliente** — e não basta ignorá-lo, é
  preciso não oferecer o campo. A 1ª versão herdava de `GridifyQuery` com `PostId` sobrescrito
  pelo controller: funcionava, mas o Swagger listava `postId (path)` **e** `PostId (query)`,
  um campo preenchível que o servidor descartava em silêncio. **Setter privado não resolve**
  (o ApiExplorer continua listando). A correção foi **composição no lugar de herança**:
  `ListarComentariosQuery(Guid postId, IGridifyQuery consulta)`, o controller binda só o
  `GridifyQuery`. Herdar do tipo bindado só é seguro quando ele descreve *tudo* que o objeto
  é — como em `ListarPostsQuery` (Fatias 13/19), onde não há nada do servidor misturado.
  *Achado pelo usuário olhando o Swagger, não por teste.*
- **Esconder ≠ apagar.** Ocultar = `Aprovado = false` na entidade rastreada (UPDATE sai no
  `UnitOfWorkBehavior`), reversível, preserva o texto. Deletar = `Remove` + cascata do
  `parent_id` leva as respostas junto. Nenhum dos dois precisou esconder/apagar respostas
  uma a uma: a listagem só devolve respostas de raízes visíveis — **modelagem certa da
  leitura encolhe a escrita**.

## Estado

Build verde (8 proj, 0 erro), 33 testes unitários verdes (a fatia não trouxe função pura
nova). 4 testes de integração novos **escritos e compilando, não executados** — sem Docker
nesta máquina. O seed só tem admins, então o teste de 403 cria o usuário comum na hora.
**Ticket 07 fechado no código** (7 de 7 itens).

## Próximo

Ticket 08 — likes anônimos. O `viewer_hash` da Fatia 20 volta, agora como chave de
unicidade no banco (um like por visitante por post) em vez de estado em memória.
