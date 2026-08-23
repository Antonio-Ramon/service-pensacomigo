---
name: criar-pr-resumo
description: 'Gera a descrição de um Pull Request de forma simples, direta e de fácil entendimento, agrupada por módulo/feature. Use quando o usuário pedir para criar a descrição de um PR ou diser "cria o PR", "gera a descrição do PR", "criar-pr-resumo", "create-pr-resume".'
---

# Skill: Criar Descrição de Pull Request

## Pré-requisito Obrigatório (ponytail)
**Antes de qualquer outra coisa**, você **DEVE** invocar a skill `ponytail:ponytail` (via ferramenta Skill) e manter seus princípios ativos durante toda a execução desta skill. A descrição do PR resultante deve ser a mais enxuta possível: bullets mínimos e diretos, sem redundância, sem repetir a mesma ideia em itens diferentes — máximo de clareza com o mínimo de palavras.

## Objetivo

Gerar uma descrição de Pull Request clara, simples e objetiva, agrupando as alterações por módulo ou funcionalidade, no padrão adotado pelo projeto.

O resultado **deve sempre** ser salvo como arquivo `.md` no diretório `_bmad-output/` do workspace com o nome `pr-description-[data-hoje].md`, e exibido como artifact ao usuário — pronto para copiar e colar no GitHub, Azure DevOps ou qualquer outra plataforma.

## Regras Gerais

- **Idioma:** Escreva sempre em **português brasileiro**.
- **Tom:** Direto e acessível a **usuários não técnicos**. Descreva o efeito da mudança para quem usa o sistema, não a implementação. Sem rodeios, sem texto introdutório.
- **Sem links de acesso:** Não inclua URLs de ambiente/rota. Apenas o conteúdo das mudanças.
- **Sem comentários desnecessários:** Não explique o que você vai fazer, apenas faça.
- **Sem blocos de código** na saída final: o resultado é Markdown puro.

---

## Como Coletar as Informações

Antes de gerar a descrição, você **deve** obter as mudanças do PR. **A fonte da verdade é SEMPRE o diff dos arquivos alterados**, nunca as mensagens de commit — commits podem mentir, estar incompletos, conter mensagens genéricas ou ter sido revertidos parcialmente em commits posteriores. O diff acumulado representa o estado real do PR.

Ordem de prioridade:

1. **Se o usuário forneceu um diff, lista de arquivos alterados ou descrição direta:** use diretamente.
2. **Se o repositório estiver disponível no workspace (PRIORIDADE PADRÃO):**
   - Identifique o autor atual: `git config user.email` (ex.: `antonio-ramon-dev@outlook.com`).
   - Liste **apenas os commits do autor atual** no range da branch:
     `git log <base>..HEAD --author="<email>" --pretty=format:"%H"`.
   - A partir desses commits, obtenha **somente os arquivos tocados pelo autor**:
     `git log <base>..HEAD --author="<email>" --name-only --pretty=format:"" | sort -u`.
   - Para cada arquivo dessa lista, inspecione o diff agregado **restrito aos commits do autor** usando o intervalo de hashes. Estratégia recomendada:
     - Capture os hashes dos commits do autor (ex.: `H1 H2 H3`).
     - Use `git diff <primeiro-pai-de-H1>^..<último-hash> -- <arquivo>` OU, quando os commits forem não-contíguos, faça `git show <hash> -- <arquivo>` por commit e una a leitura.
   - **Nunca** use `git diff <base>..HEAD` puro: isso inclui commits de terceiros (merges trazidos por rebase, contribuições mescladas, etc.) que **não pertencem a este PR conceitual**.
   - `git log` pode ser usado como contexto auxiliar (entender intenção de uma mudança ambígua), mas **a fonte primária** continua sendo o conteúdo dos diffs do autor.
3. **Se nenhuma das opções acima funcionar:** pergunte ao usuário:
   - "Pode colar o diff ou a lista de arquivos alterados desse PR?"

### Regras de uso do diff

- **Considere apenas o que foi alterado pelo autor atual.** Arquivos tocados exclusivamente por outros autores (mesmo presentes no range `<base>..HEAD`) **não** entram no PR resumo.
- **Confira o que está realmente no arquivo final** dentro dos commits do autor, não no que cada commit individual disse fazer. Mudanças podem ter sido revertidas, refinadas ou substituídas.
- **Ignore commits sem efeito no diff final.** Se o diff dos commits do autor não mostra alteração em um arquivo, ele não entra no PR — mesmo que uma mensagem de commit tenha mencionado.
- **Não invente módulos baseado em commits de terceiros ou em mensagens.** Se o diff do autor não tem nada em `pages/crm/`, **não** mencione CRM no PR.
- **Para arquivos `.scss` ou `.html` pequenos**, leia o diff antes de descrever — não confie apenas no nome do arquivo ou na intuição.
- **Em caso de dúvida sobre autoria** (ex.: branch compartilhada, rebases, co-autoria), confirme com o usuário antes de incluir ou descartar arquivos.

---

## Processo de Análise

1. **Identifique o autor atual** (`git config user.email`) e liste **apenas os arquivos tocados por esse autor** no range da branch (via `git log <base>..HEAD --author="<email>" --name-only`). Essa lista — não `git diff --stat <base>..HEAD` — é a base de tudo.
2. **Para cada arquivo, inspecione o diff** restrito aos commits do autor antes de descrever. Se o nome do arquivo não revela a mudança (típico em `.scss` / `.html`), abra o diff. Não infira pelo nome.
3. **Gere um título** curto e descritivo para o PR (máx. 80 caracteres), resumindo o conjunto das mudanças do autor. Exemplos: `feat: modernização do orquestrador de matrícula e filtros de receitas`, `fix: correções pós-treinamento e ajustes no fluxo de matrícula`.
4. **Agrupe as mudanças** por módulo, feature ou domínio funcional (ex: Notificações, Receitas, Matrículas, Feed, etc.) — derivado dos caminhos dos arquivos alterados pelo autor, não de commits ou mensagens.
5. **Escreva bullets concisos** descrevendo O QUE foi feito (não como foi implementado).
6. Use nomes de arquivos, componentes ou serviços apenas quando agregarem contexto (ex: "`FeedService`", "`ew-date-range`") — e sempre acompanhados de uma explicação em linguagem comum do que a mudança significa para o usuário.
7. **Desconsidere Merge Commits, commits de terceiros e commits sem efeito final:** se o diff dos commits do autor não mostra alteração, a mudança não existe — independentemente de quantos commits a citaram.
8. **Um grupo por seção** separada por `---`.

---

## Formato de Saída

O output deve seguir **exatamente** este padrão, com o título obrigatório no topo:

```
# [Título conciso do PR]

## [Nome do Módulo / Feature]
- [Bullet descrevendo a mudança]
- [Bullet descrevendo a mudança]

---
## [Nome do Módulo / Feature]
- [Bullet descrevendo a mudança]

---
```

### Exemplo Real

```
# feat: refinamento em notificações, atendimentos e feed

## Notificações
- Refinamento no sistema de notificações: novos métodos no `FeedService` e `NotificationPageService`
- Utilitário `NotificationUtils` adicionado
- Componente `header-notification` aprimorado

---
## Atendimentos
- Ajuste no filtro e paginação em atendimentos
- Quebra de linha em palavras grandes nas mensagens (evita estouro de layout)

---
## Grupos
- Remoção de membros já selecionados da lista ao editar grupo

---
## Feed
- Botão de editar escondido no `ew-sidebar` do feed caso o mesmo já tenha sido publicado

---
```

---

## Regras de Qualidade

- Cada bullet deve ser **auto-explicativo** sem precisar abrir o código.
- Prefira verbos no passado: "Adicionado", "Corrigido", "Ajustado", "Removido", "Refinado".
- Se uma mudança for um bugfix, deixe explícito: "Correção de..." ou "Fix: ...".
- Se for refactoring sem impacto visual, indique: "Refatoração de..." ou "Melhoria interna em...".
- Agrupe itens muito pequenos do mesmo contexto em um único bullet quando fizer sentido.
- **Linguagem para não técnicos:** cada bullet deve ser compreensível por alguém que não programa (ex.: gestor, analista de suporte). Evite jargão de implementação (handler, DTO, regex, middleware, injeção de dependência); descreva o comportamento percebido no sistema (ex.: "Ao encerrar uma conta com vínculos, o sistema agora informa exatamente qual vínculo impede o encerramento").
- **Máximo de clareza, mínimo de palavras.**

---

## Entrega Final

Após montar a descrição:

1. **Salve** o conteúdo como `_bmad-output/pr-description-[YYYY-MM-DD].md` (use a data atual no nome).
2. **Exiba como artifact** para o usuário — o conteúdo deve ser Markdown puro, sem envolver em bloco de código, para que seja renderizado e copiável diretamente.
3. Informe ao usuário que o arquivo foi salvo e onde está, em uma linha curta após o artifact.
