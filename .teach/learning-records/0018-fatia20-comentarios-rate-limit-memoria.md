# 0018 — Fatia 20: comentários, estado em memória e tempo de vida no DI

**Data:** 2026-08-07 · **Aula:** [0020](../lessons/0020-comentarios-rate-limit-memoria.html) · **Ticket:** 07 (1ª metade)

## O que foi aprendido

- **Nem todo estado é banco.** Cinco carimbos que expiram em 1 min não merecem tabela:
  `IMemoryCache` + `cache.Set(chave, valor, Janela)` — a expiração absoluta faz a faxina,
  sem rotina de limpeza e sem memória crescendo por visitante.
- **Tempo de vida no DI (primeiro `Singleton` do projeto).** `Scoped` = por requisição
  (`DbContext`, repositórios, handlers); `Singleton` = por aplicação. O limitador precisa
  sobreviver *entre* requisições — `Scoped` nasceria vazio e o limite nunca bateria.
- **Captive dependency:** `Singleton` nunca injeta `Scoped`. Guardaria o `DbContext` da
  primeira requisição para sempre — não thread-safe, dados velhos. O contêiner detecta na
  subida quando `ValidateScopes` está ligado (padrão em Development).
- **Relógio como parâmetro.** `JanelaDeslizante.Registrar(carimbos, agora, janela, maximo)`
  é pura: o teste *afirma* que horas são em vez de dormir 61 s. Mesmo movimento do
  `GeradorSlug` (Fatia 15). Retorno `List<DateTime>?` onde `null` **é** a resposta
  "estourou"; o `??` no chamador vira exceção numa linha. (`TimeProvider`, .NET 8+, é a
  alternativa quando a dependência de tempo não dá pra empurrar pra borda.)
- **Validator vs handler, com critério.** O validator vê **só o command** — palavrão cabe
  ali. Regra que precisa ler o banco (o comentário pai é raiz?) é do handler. Guardar essa
  fronteira é o que impede o validator de virar um segundo handler.
- **1 nível é política, não schema.** `parent_id` aceita profundidade infinita de propósito
  (Decisão #7): o banco garante integridade, a política de produto fica no código.
- **Identidade do visitante é do servidor** (`HashVisitante`: SHA-256 de IP + User-Agent).
  Mesma lição do path da imagem (Fatia 14): quem manda a própria identidade só precisa
  trocá-la para zerar o limite. O hash também evita guardar IP cru (dado pessoal).
- **Resposta montada antes do commit não vê valor gerado pelo banco.** `data_criacao` tem
  `default now()` e o `UnitOfWorkBehavior` commita *depois* do handler — por isso
  `ComentarioResponse` não tem esse campo.
- **Teste que depende de estado global precisa isolar a chave.** No `WebApplicationFactory`
  não há IP nem User-Agent reais → todos os clientes gerariam o mesmo hash e dividiriam o
  balde. Cada teste ganha um User-Agent único.
- `MuitasRequisicoesException` → 429 no `GlobalExceptionHandler`: fecha o status que ficou
  de fora da Fatia 6.

## Estado

Build verde (8 proj, 0 erro), **33 testes unitários verdes** (18 anteriores + 15 novos:
`FiltroPalavrao` e `JanelaDeslizante`). 6 testes de integração novos **escritos e
compilando, não executados** — sem Docker nesta máquina. Issue 07 com 5 de 7 itens fechados.

Um teste unitário saiu errado na primeira tentativa (assumiu que 5 carimbos em segundos
diferentes ainda estariam vigentes 59 s depois — os mais velhos já tinham expirado). O
código estava certo; o teste é que mediu a coisa errada.

## Próximo

Fatia 21 — fechar o Ticket 07: listar comentários aprovados com respostas via Gridify
(árvore rasa em resposta JSON) e moderação por admin. Primeira autorização por **claim**
(`is_admin`), não só por "tem token".
