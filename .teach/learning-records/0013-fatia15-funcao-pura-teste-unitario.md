# 0013 — Fatia 15: função pura + primeiro teste unitário

**Data:** 2026-08-03 · **Aula:** `lessons/0015-funcao-pura-teste-unitario.html` · **Issue:** 05 — Post (criar/editar/deletar)

## Contexto
Issue 05 é grande (7 itens). Dois vêm marcados `[unit]` — slug e tempo de leitura — e são
justamente a parte do caso de uso sem I/O. Fatiar por aí adiantou o Ticket 05 **e** deu o
primeiro teste que roda nesta máquina (Docker ausente desde a Fatia 8).

## O que foi aprendido
- **Função pura**: resultado só dos argumentos, zero efeito colateral. Testável sem fake, sem
  banco, sem rede. O padrão é o *impureim sandwich*: handler lê (impuro) → função pura decide →
  handler grava (impuro).
- **Pirâmide de testes**: `UnitTests` (ms, sem infra, testa a REGRA) vs `IntegrationTests`
  (segundos, Docker, testa a FIAÇÃO). O teste de integração do ticket vai provar jsonb + índice
  único, não a matemática do slug.
- **xUnit**: `[Fact]` = 1 caso; `[Theory]` + `[InlineData]` = N testes independentes de um método
  (11 métodos → 18 testes). Sem `[SetUp]`/`[TearDown]`: instância nova por teste, construtor é o
  setup, `IDisposable`/`IAsyncLifetime` é o teardown.
- **Testar a borda, não o meio**: 200 e 201 palavras provam o arredondamento; 50 e 400 não provam nada.
- `Split((char[]?)null, RemoveEmptyEntries)` separa por qualquer whitespace — `Split(' ')` deixaria
  passar `\n` do HTML.

## Decisões
- **`GeradorSlug` foi para `Application/Common/`, não para o `Shared`** — desvio consciente do
  texto do ticket. `Shared → Application` é a direção da seta; código no Shared seria invisível
  para o handler (referência circular). Regra: **quem decide onde o código mora é a direção da
  dependência, não o ticket**. (2º desvio registrado; o 1º foi o path da imagem, Fatia 14.)
- **`ResolverColisao(slugBase, ocupados)` recebe a lista** em vez de consultar. Um round-trip no
  handler ("slugs com prefixo X"), decisão pura e testável. A alternativa (`while` + `await`)
  seria N idas ao banco e intestável sem Postgres.
- **Tag não resolve colisão com `-2`** — nome equivalente é a MESMA tag, então segue 422
  (Fatia 12). Só Post ganha sufixo.
- `CriarTagCommandHandler` perdeu a cópia local da normalização e passou a chamar
  `GeradorSlug.Gerar`. Uma regra, um lugar.
- `PalavrasPorMinuto = 200` como const com `ponytail:` — vira config só se alguém reclamar.

## A revisitar
- **`ResolverColisao` não substitui o índice único**: dois posts simultâneos leem a mesma lista e
  escolhem o mesmo `-2`. A checagem é pro erro amigável; a constraint é quem garante.
- Entidades HTML (`&nbsp;`, `&amp;`) contam como palavra hoje — irrelevante no arredondamento
  por minuto, mas está aqui registrado.
- `TempoLeitura` ignora blocos de imagem/link (só mínimo de 1 min). Se o editor gerar posts
  majoritariamente visuais, vale somar segundos por imagem.
