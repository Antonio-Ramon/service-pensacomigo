# ADR 0001 — Realtime de comentários e curtidas via SignalR + notification MediatR

**Data:** 2026-08-01
**Status:** aceito

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
- **Fonte do evento: o próprio handler MediatR.** Todo comentário/like já passa por um
  `Command`. Após o commit, o handler publica uma **notification** (ex.
  `ComentarioCriadoNotification`); um `NotificationHandler` empurra para o Hub no grupo
  `post:{id}`. Não escutamos o banco.

Descartados:
- **Postgres LISTEN/NOTIFY** — só valeria se algo além do nosso backend escrevesse nas
  tabelas; só nós escrevemos.
- **Backend consumindo o Supabase Realtime** — traz de volta o acoplamento que se quis evitar.

## Consequências

- **RLS continua desnecessário** para o realtime: o frontend nunca toca o Supabase,
  só o nosso WS. Controle de acesso permanece na camada Application (`[Authorize]` +
  handlers). RLS só entra se algum dia o frontend acessar o Supabase diretamente.
- Encaixa no pipeline MediatR já existente (Fatia 5): o evento nasce no caso de uso.
- **Escala horizontal exige backplane.** Com mais de uma instância do backend, um evento
  criado na instância A não chega aos clientes conectados na instância B sem um
  **Redis backplane** (`Microsoft.AspNetCore.SignalR.StackExchangeRedis`) — uma linha de
  config. Adiado enquanto rodar instância única.
  `// ponytail: single-instance agora; Redis backplane só quando escalar pra N instâncias`

## Desenho

```
FE  ──(WebSocket/SignalR + JWT)──►  ComentariosHub / grupo "post:{id}"
                                          ▲
CriarComentarioCommandHandler ─commit──► publica ComentarioCriadoNotification (MediatR)
                                          │
                            NotificationHandler ──► Hub.Clients.Group("post:{id}").Send(...)
```
