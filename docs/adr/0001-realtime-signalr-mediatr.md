# ADR 0001 — Realtime de comentários e curtidas via SignalR + notification MediatR

**Data:** 2026-08-01
**Status:** aceito
**Revisado em:** 2026-09-20 — duas vezes. (1) o mecanismo de emissão do evento foi corrigido
(ver *Decisão*, *Fonte do evento*). (2) o escopo passou de comentário para curtida, visualização
e feed (Fatia 25): hub renomeado e a regra do payload explicitada. Transporte e fonte inalterados.

## Contexto

Comentários e curtidas precisam aparecer em tempo real para quem está com o post
aberto. A alternativa "óbvia" no Supabase é o **frontend escutar as tabelas direto**
(supabase-js + Realtime), mas isso:

- acopla o frontend ao Supabase e ao schema do banco;
- **obriga a ligar RLS** (Row Level Security) em todas as tabelas, senão o `anon key`
  expõe leitura/escrita direta — e hoje as tabelas foram criadas pelo EF sem RLS
  (ver migration `InicialSchema`, nenhuma policy);
- fura a arquitetura "backend na frente" (todo acesso passa pela nossa API).

## Decisão

Realtime é servido por um **WebSocket do nosso backend**, não pelo Supabase.

- **Transporte FE ↔ backend: SignalR** (nativo ASP.NET Core, zero dependência nova).
  Abstrai WebSocket com fallback (SSE/long-polling), tem **grupos** (`post:{id}` —
  só quem abriu o post recebe), autentica a conexão com o **JWT próprio** e reconecta
  sozinho.
- **Fonte do evento: o caso de uso, não o banco.** Todo comentário/like já passa por um
  `Command`; o evento nasce ali. Mas **quem o emite é a pipeline, depois do commit** —
  nunca o handler. O handler só **enfileira** a notification (`FilaDeEventos`, scoped);
  um `DespachoDeEventosBehavior` que **envolve** o `UnitOfWorkBehavior` drena a fila
  depois que o `CommitAsync` voltou, e aí sim publica. O `NotificationHandler` empurra
  para o Hub no grupo `post:{id}`. Não escutamos o banco.

  A redação anterior deste ADR dizia que "o handler publica após o commit". Isso é
  impossível neste desenho: o handler **não commita** — quem commita é o
  `UnitOfWorkBehavior`, que roda envolvendo o handler (`var r = await next(ct);
  await uow.CommitAsync(ct);`). Todo o corpo do handler é, por construção, pré-commit.
  Pior: `AdicionarAsync` só mexe no ChangeTracker, então um push emitido lá anunciaria
  uma linha que **nenhum INSERT levou ao Postgres ainda** — o cliente recebe o evento,
  refaz o `GET` e não acha nada. E se o commit falhar, o grupo inteiro já foi notificado
  de algo que nunca existiu.

Descartados:
- **Postgres LISTEN/NOTIFY** — só valeria se algo além do nosso backend escrevesse nas
  tabelas; só nós escrevemos.
- **Backend consumindo o Supabase Realtime** — traz de volta o acoplamento que se quis evitar.

## Consequências

- **RLS continua desnecessário** para o realtime: o frontend nunca toca o Supabase,
  só o nosso WS. Controle de acesso permanece na camada Application (`[Authorize]` +
  handlers). RLS só entra se algum dia o frontend acessar o Supabase diretamente.
- Encaixa no pipeline MediatR já existente (Fatia 5): o evento nasce no caso de uso.
- **A ordem de registro dos behaviors passa a ser a regra inteira.** `AddOpenBehavior`
  vai do mais externo pro mais interno, então `DespachoDeEventosBehavior` tem que ser
  registrado **antes** do `UnitOfWorkBehavior` para envolvê-lo. Inverter as duas linhas
  reintroduz o push pré-commit sem nenhum sinal — merece teste que trave: comando que
  estoura no commit não pode produzir evento.
- **A curtida não fecha só com pós-commit.** `AjustarCurtidasAsync` usa `ExecuteUpdate` e
  grava **fora** do commit do behavior (ver o `ponytail` em `CurtirPostCommandHandler`):
  o INSERT do like espera o commit, o contador não. O número empurrado pelo WS pode
  divergir do que o `GET` devolve até isso virar uma transação explícita.
- **Pós-commit in-process não é entrega garantida**: processo que morre entre o commit e
  o publish grava o dado e perde o evento. Outbox transacional é overkill aqui — a
  mitigação é o front **refazer o `GET` ao reconectar** em vez de confiar só no stream
  (reconexão do SignalR também não recoloca a conexão nos grupos).
- **Escala horizontal exige backplane.** Com mais de uma instância do backend, um evento
  criado na instância A não chega aos clientes conectados na B sem um
  **Redis backplane** (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) — uma linha de
  config. Adiado enquanto rodar instância única.
  `// ponytail: single-instance agora; Redis backplane só quando escalar pra N instâncias`

## Desenho

```
FE  ──(WebSocket/SignalR)──►  TempoRealHub / grupo "post:{id}" (ou Clients.All, no feed)
                                          ▲
                                          │
DespachoDeEventosBehavior                 │
  └─ UnitOfWorkBehavior                   │
       ├─ CriarComentarioCommandHandler   │
       │    ├─ AdicionarAsync()           │   só ChangeTracker
       │    └─ fila.Adicionar(evento)     │   só enfileira
       └─ CommitAsync()                   │   INSERT confirmado
  └─ drena a fila ─► Publish ─► NotificationHandler ─► Hub.Clients.Group(...)
```
