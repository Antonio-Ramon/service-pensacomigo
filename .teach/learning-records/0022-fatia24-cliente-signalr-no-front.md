# 0022 — Fatia 24: o cliente SignalR no front

**Data:** 2026-09-20 · **Aula:** [0024](../lessons/0024-cliente-signalr-no-front.html) · **ADR:** [0001](../../docs/adr/0001-realtime-signalr-mediatr.md)
**Repo:** `front-pensacomigo` (a outra metade da [Fatia 23](0021-fatia23-signalr-eventos-pos-commit.md))

## O que foi aprendido

- **O endpoint do Hub não é um WebSocket cru.** Antes vem `POST /hubs/comentarios/negotiate`
  (devolve `connectionId` + transportes), depois o handshake `{"protocol":"json","version":1}`,
  e as mensagens são frames terminados em `\u001e`. Falar isso na mão foi o experimento em Python
  da Fatia 23 — serve para provar que o servidor está de pé, não para escrever cliente.
  `@microsoft/signalr` é o cliente oficial do mesmo protocolo: negotiate, handshake, fallback
  (SSE/long polling) e reconexão com backoff. **Única dependência nova da fatia.**
- **Três strings atravessam a fronteira sem compilador nenhum vigiando**: a rota
  (`"/hubs/comentarios"`, nasce no `MapHub`), o método (`"Entrar"`, método `public` do Hub) e o
  evento (`"ComentarioCriado"`, 1º argumento do `SendAsync`). O front inteiro é tipado a partir do
  OpenAPI — **essas três não**. Errar qualquer uma é silencioso: nada estoura, nada chega.
- **O push é sinal, não dado.** `ComentarioResponse` (empurrado) não tem `DataCriacao` (coluna com
  `default now()`: no instante do evento o valor ainda não voltou do banco), `AutorImagemUrl`,
  `EhAutorDoPost` nem `Respostas` — que é o que `ComentarioListaResponse` (a tela) consome.
  Inventar os quatro é aceitável no envio otimista (quem escreveu sabe quem é) e seria chute no
  comentário de outra pessoa. Então: chegou evento → refaz o `GET`.
  Isso também é o que conserta o furo do pós-commit in-process (evento perdido não deixa a tela
  permanentemente errada).
- **Conexão é recurso: vive num `useEffect` e morre no cleanup.** Sem o `stop()`, trocar de post
  deixa a conexão antiga viva — o leitor segue no grupo anterior e recebe comentário de post que
  não está na tela. O `StrictMode` (monta/desmonta/remonta em dev) expõe justamente isso, e é por
  ele que o `stop()` também leva `.catch()`: pode chegar no meio do negotiate.
- **`catch` vazio no `start()` é deliberado**: API fora, rede bloqueando WS, extensão atrapalhando
  → a página continua funcionando com `GET` + formulário. Realtime é melhoria, não requisito.
  (Contraste com a Fatia 23, onde a **ausência** de `try/catch` é que era a garantia. O critério é
  o mesmo: engolir erro só onde o caminho degradado é aceitável.)
- **Reconectar não é reentrar.** `withAutomaticReconnect()` devolve a conexão, não os grupos: o
  grupo guarda `ConnectionId` e o reconnect gera outro. `onreconnected` faz **duas** coisas —
  reinvoca `Entrar` (senão o resto da sessão é silêncio) e refaz o `GET` (senão os comentários da
  janela offline não existem para esta tela).

## Estado

`npm run build` verde. `Comentarios.tsx`: `carregar` extraído em `useCallback` e reusado por três
caminhos (mount, evento, reconnect); efeito de conexão separado do efeito da sessão.
Sem hook `useHubComentarios` — um único consumidor (YAGNI).

## Não verificado

- **Nada rodou ponta a ponta nesta sessão.** Só build + typecheck. O e2e exigiria a API local
  contra o **Supabase de produção** (grava comentário real) e o usuário optou por fechar a fatia
  sem isso. A fiação equivalente do lado do servidor **foi** verificada na Fatia 23
  (duas conexões, grupo certo recebeu, grupo errado não).
- Reconexão (`onreconnected`) nunca foi exercitada em nenhuma das duas fatias.

## Ressalvas em aberto

- Curtida sem realtime, visualização sem push, backplane Redis adiado — todas herdadas do
  [LR 0021](0021-fatia23-signalr-eventos-pos-commit.md), nenhuma mexida aqui.
