# 0023 — Fatia 25: fan-out, sinal ou valor

**Data:** 2026-09-20 · **Aula:** [0025](../lessons/0025-fanout-sinal-ou-valor.html) · **ADR:** [0001](../../docs/adr/0001-realtime-signalr-mediatr.md) (revisado 2ª vez)
**Repos:** `service-pensacomigo` + `front-pensacomigo`

## O que foi aprendido

- **"O push é sinal, o `GET` é a verdade" (Fatia 24) não é regra universal — é consequência.**
  Ela vale enquanto refazer o `GET` for barato *e sem efeito colateral*, e enquanto o payload
  empurrado for incompleto. Na página do post as duas falham: o `GET` **é** o
  `AbrirPostCommand`, que **incrementa visualização**, e um `int` não tem como mentir. Por isso
  `CurtidasAtualizadas` e `PostVisualizado` carregam o **valor**; comentário e feed carregam o
  **sinal**. A pergunta que decide não é "é tempo real?", é *"o cliente consegue redescobrir
  isso sozinho, sem estragar outra coisa?"*.
- **`ExecuteUpdateAsync` devolve linhas afetadas, não valores.** Para empurrar o número certo
  sem uma segunda ida ao banco (e sem a corrida entre `UPDATE` e `SELECT`), o repositório passou
  a usar SQL cru com `RETURNING qtd_curtidas`. Interpolação em `FormattableString` vira
  **parâmetro**, não concatenação — o mesmo código com `string` comum nem compila.
- **O `null` virou informação.** Nenhuma linha casou = o contador não mudou (post inexistente,
  ou a guarda `qtd_curtidas + delta >= 0` barrou o descurtir a mais) = não há o que anunciar.
  Travado em `EventoDeCurtidaTests`, **verificado falhando** ao trocar o `if` por `qtd ?? 0`.
- **Grupo é para quem abriu alguma coisa.** Quem está no feed não abriu post nenhum e não está em
  grupo algum → `PostPublicado`/`PostRemovido` vão em `Clients.All`. Com cinco eventos o hub
  deixou de ser dos comentários: `ComentariosHub` → `TempoRealHub`, `/hubs/tempo-real`.
  **Um hub só de propósito**: cada hub é uma conexão WebSocket por aba.
- **O mesmo argumento cobrou a fatura no front.** A página do post tem três componentes ouvindo;
  com o desenho da Aula 24 seriam três WebSockets. A conexão subiu para `src/lib/tempoReal.ts` e
  o que entra/sai com o componente virou **ouvinte e grupo**. De brinde sumiu o
  `"connection was stopped during negotiation"` (não há mais `stop()`); em troca, o
  `onreconnected` agora reentra em **todos** os grupos ativos — o módulo guarda esse conjunto
  porque nenhum componente sozinho sabe dele.
- **`router.refresh()` em vez de duplicar a listagem no cliente.** O feed é server component:
  o ouvinte só pede o re-render e o servidor continua sendo a fonte. E **por isso**
  `<FeedAoVivo />` fica nas três páginas de feed, não no layout público — na página do post um
  refresh refaria o `abrirPost` e incrementaria visualização. A armadilha da primeira seção
  entrando pela porta dos fundos.
- **Despublicar é, para quem olha o feed, o mesmo fato que deletar** → `EditarPostCommandHandler`
  emite `PostRemovido` na transição Publicado → não-publicado, e `PostPublicado` na inversa. O
  status anterior precisa ser lido **antes** de sobrescrever.

## Correção de duas afirmações anteriores

1. O ADR dizia que "o número de curtidas empurrado pelo WS pode divergir do que o `GET` devolve".
   **Falso.** O contador mora numa coluna; WS e `GET` leem a mesma. O drift real é
   `qtd_curtidas` × `COUNT(likes)` e existe com ou sem realtime. A curtida nunca esteve
   bloqueada para realtime por esse motivo.
2. O LR 0021 listava a curtida entre as "ressalvas em aberto" por consequência disso. Cai junto.

## Estado

Build verde (8 proj, 0 erro; warnings MSB3277/NU1903 pré-existentes). **68 testes unitários
verdes** (64 + 4). Front: `npm run build` e `tsc --noEmit` verdes.

Eventos novos: `CurtidasAtualizadas(PostId, Qtd)`, `PostVisualizado(PostId, Qtd)`,
`PostPublicado(PostId, Slug)`, `PostRemovido(PostId, Slug)` — os dois primeiros no grupo do post,
os dois últimos em `Clients.All`. Cinco `AddScoped<INotificationHandler<…>>` no `Program.cs`.

## Não verificado

- **Nada rodou ponta a ponta**, de novo: o e2e exigiria a API local contra o Supabase de
  produção. Só build + testes unitários + typecheck do front.
- Reconexão (agora com reentrada em N grupos) segue sem nunca ter sido exercitada.
- O `RETURNING` não passou por banco real nesta sessão — o teste unitário usa fake de repositório
  e os testes de integração não rodam sem Docker. **É o ponto mais frágil da fatia.**

## Ressalvas em aberto

- **Agendado que vence não emite nada**: o post entra no ar porque a data passou, sem comando
  nenhum. Precisaria de job.
- **Quem está lendo um post deletado não é avisado** — o evento chega e a página o ignora.
- **Visualização empurra 1 mensagem por leitor que abre** (`ponytail:` no handler). Upgrade =
  agregar num `PeriodicTimer`.
- Backplane Redis segue adiado (instância única).
