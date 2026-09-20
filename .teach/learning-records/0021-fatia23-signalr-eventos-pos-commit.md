# 0021 — Fatia 23: SignalR e eventos pós-commit

**Data:** 2026-09-20 · **Aula:** [0023](../lessons/0023-signalr-eventos-pos-commit.html) · **ADR:** [0001](../../docs/adr/0001-realtime-signalr-mediatr.md)

## O que foi aprendido

- **SignalR é nativo do ASP.NET Core** — `AddSignalR()` + `MapHub<T>("/rota")`, zero pacote novo.
  Hub = classe cujos métodos `public` o browser invoca; **grupo** = nome + lista de `ConnectionId`;
  `IHubContext<T>` = como o resto do servidor empurra mensagem de fora de uma chamada do Hub.
- **O corpo do handler é, por construção, pré-commit.** `AdicionarAsync` só toca o ChangeTracker;
  quem manda o INSERT é o `UnitOfWorkBehavior`, que **envolve** o handler. Push emitido de dentro
  anuncia linha que não existe (cliente refaz o `GET` e não acha) e, se o commit falhar, notifica
  algo que nunca existiu.
- **A solução é ordem, não código**: o handler *enfileira* (`FilaDeEventos`, Scoped), e um
  `DespachoDeEventosBehavior` registrado **antes** do `UnitOfWorkBehavior` drena depois que o
  `CommitAsync` voltou. `AddOpenBehavior` monta do mais externo pro mais interno.
- **A garantia é a ausência de `try/catch`.** Commit que estoura sobe a exceção e o `foreach` que
  drena nunca é alcançado. Um `catch` bem-intencionado ali reintroduz o bug silenciosamente.
- **Regra invisível precisa de teste que a trave.** Inverter as duas linhas do registro compila,
  sobe e passa em todo teste de rota. `Despacho_e_registrado_por_fora_do_unit_of_work` inspeciona a
  `ServiceCollection` do `AddApplication` — foi conferido que ele **falha** com a ordem invertida.
  Testar a cebola montada à mão não bastava: ela não vê o registro real.
- **A outra metade do MediatR**: `IRequest`/`Send` = exatamente 1 handler, devolve resposta,
  nome no imperativo. `INotification`/`Publish` = 0, 1 ou N handlers, não devolve nada, nome no
  **passado**. Comando pode ser recusado; evento já aconteceu — por isso só nasce pós-commit.
- **`Publish` com zero handlers não é erro** (ao contrário do `Send`). O handler do Hub mora no
  `Web` (é `IHubContext`, a Application não o conhece) e o scan do MediatR varre a *Application* →
  não é encontrado, tudo passa e nada chega na tela. Uma linha explícita de `AddScoped` resolve;
  escanear a assembly do Web puxaria todo o host pro scan por causa de um handler.
- **Duas bordas do transporte**: a allowlist do CORS precisa de `x-signalr-user-agent` (o
  `/negotiate` morre num erro que não cita SignalR); e `withAutomaticReconnect` devolve a conexão,
  **não** os grupos — o grupo vive por `ConnectionId`, que muda no reconnect.
- **Pós-commit in-process não é entrega garantida**: processo que morre entre commit e publish
  grava o dado e perde o evento. Outbox é overkill; o front trata o stream como aceleração e o
  `GET` como verdade.

## Estado

Build verde (8 proj, 0 erro; warnings MSB3277/NU1903 pré-existentes). **64 testes unitários
verdes** (61 + 3 novos), rodam sem Docker. `POST /api/v1/posts/{id}/comentarios` publica no grupo
`post:{postId}` do hub `/hubs/comentarios`, evento `ComentarioCriado` com o `ComentarioResponse`.

Hub **anônimo** de propósito. Nenhum teste de integração novo — o que importava (ordem + ausência
de publish no commit falho) é unitário.

## Verificado rodando (API local na 5001)

Cliente SignalR escrito no cru (stdlib do Python: HTTP Upgrade → handshake
`{"protocol":"json","version":1}` → `{"type":1,"target":"Entrar"}`), duas conexões em grupos
diferentes, comentário criado por `POST` HTTP comum:

- **A (grupo do post) recebeu 1, B (outro grupo) recebeu 0** → `Entrar` e `Handle` executam
  apesar dos "0 referências" no IDE, e o filtro por grupo funciona (não é `Clients.All`).
- `OPTIONS /hubs/comentarios/negotiate` devolve **204** com `x-signalr-user-agent` liberado;
  **controle negativo**: um header fora da allowlist não aparece na resposta → a mudança de CORS
  é load-bearing. `POST /negotiate` lista `WebSockets, ServerSentEvents, LongPolling`.

**O que o e2e NÃO prova** (erro cometido e corrigido na sessão): a ordem pós-commit. Push
pré-commit produziria saída idêntica — a ordem das linhas no terminal é ruído de duas threads.
Quem prova a ordem é o teste unitário, verificado falhando com o registro invertido.
*Teste e2e responde "está plugado?"; teste unitário responde "a regra vale?".*

Não testado: necessidade do `AddScoped<INotificationHandler<…>>` (exigiria remover a linha e
derrubar a API) e o comportamento de reconexão.

## Ressalvas em aberto

- **Curtida ainda não tem realtime.** `AjustarCurtidasAsync` usa `ExecuteUpdate` e grava fora do
  commit (`ponytail:` da Fatia 22): o número empurrado pelo WS divergiria do `GET` até isso virar
  transação explícita. Ficou de fora conscientemente.
- **Visualização não terá push individual** — centenas/min num post viral, e o
  `AbrirPostCommandHandler` já devolve `QtdVisualizacoes + 1` (duas fontes de verdade na tela).
  Se um dia for feito: agregado por `PeriodicTimer` num `BackgroundService`.
- **Backplane Redis** adiado enquanto rodar instância única (ADR 0001).

## Próximo

O front (`~/front-pensacomigo`): `@microsoft/signalr`, `Entrar` no `onreconnected` + refetch do
`GET`. É a outra metade desta fatia e fecha o caso de uso de ponta a ponta.
