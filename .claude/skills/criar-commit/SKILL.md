---
name: criar-commit
description: 'Gera mensagens de commit seguindo o padrão Conventional Commits em português brasileiro (pt-br). Use quando o usuário pedir para criar a mensagem de commit ou disser "criar-commit", "gera commit".'
---

# Skill: Criar Mensagem de Commit (Conventional Commits)

## Pré-requisito Obrigatório (ponytail)
**Antes de qualquer outra coisa**, você **DEVE** invocar a skill `ponytail:ponytail` (via ferramenta Skill) e manter seus princípios ativos durante toda a execução desta skill. A mensagem de commit resultante deve ser a mais enxuta possível: a menor descrição que ainda comunica a mudança, sem palavras supérfluas e sem corpo de mensagem quando não for estritamente necessário.

## Objetivo
Gerar e **executar** commits intuitivos, curtos e padronizados seguindo o Conventional Commits, utilizando exclusivamente o português brasileiro para as descrições. Esta skill deve realizar a ação de commit no repositório.

## Regras do Padrão
O formato deve ser: `<tipo>(<escopo>): <descrição curta em minúsculas>`

Exemplo: `refact(side-nav): removido a parte de categorias do componente`

### Tipos Permitidos:
- **feat**: Nova funcionalidade.
- **fix**: Correção de bug.
- **docs**: Alterações apenas na documentação.
- **style**: Alterações que não afetam o significado do código (espaço, formatação, etc).
- **refact**: Alteração de código que não corrige um bug nem adiciona funcionalidade (Refatoração).
- **perf**: Melhoria de desempenho.
- **test**: Adição ou correção de testes.
- **build**: Alterações no sistema de build ou dependências externas.
- **ci**: Alterações nos arquivos de configuração e scripts de CI.
- **chore**: Outras alterações que não modificam arquivos de src ou test.
- **revert**: Reverter um commit anterior.

## Como Coletar as Informações
1. **Verificar Staged Changes**: Tente executar `git diff --cached --stat` e `git diff --cached` para entender o que está pronto para ser commitado.
2. **Analisar Contexto Recente**: Se não houver nada no stage, analise as últimas alterações feitas nos arquivos durante a sessão atual.
3. **Perguntar ao Usuário**: Se as mudanças forem ambíguas, peça uma breve descrição do que foi feito.

## Processo de Execução
1. **Identificar o Tipo**: Escolha o tipo que melhor descreve a mudança principal. Use `fix` para correções e `refact` para refatorações/alterações de estrutura.
2. **Definir o Escopo**: Identifique o módulo, componente ou serviço afetado (ex: `matricula`, `auth`, `form`). Use parênteses para o escopo.
3. **Escrever a Descrição**: Use o **particípio passado** (ex: adicionado, corrigido, otimizado, padronizado), em letras minúsculas, de forma direta e em **português brasileiro**.
4. **Executar o Comando**:
   - Se houver arquivos no stage (`git diff --cached`), execute: `git commit -m "<mensagem>"`
   - Se não houver arquivos no stage, mas houver mudanças detectadas, adicione os arquivos relevantes primeiro: `git add <arquivos>` e depois o commit.
   - **IMPORTANTE**: Informe ao usuário o comando que será executado e peça confirmação apenas se houver risco de incluir arquivos indesejados. Caso contrário, proceda com a execução.

## Formato de Saída
Após a execução, apresente o resultado do comando `git commit` e a mensagem utilizada.

Exemplo de saída:
`feat(matricula): modernizado orquestrador para usar signals`

---
## Regras Adicionais
- **Atomicidade e Contextos**: Se as alterações abrangerem múltiplos componentes ou módulos não relacionados (ex: uma alteração em `select` e outra em `card`), você **DEVE** separar as mudanças em commits distintos.
- **Agrupamento**: Identifique os grupos de arquivos por afinidade técnica ou funcional. Realize o `git add` apenas dos arquivos de um contexto e execute o commit antes de passar para o próximo grupo.
- **Assunto Curto**: Mantenha a primeira linha abaixo de 50-72 caracteres.
- **Sem Pontuação**: Não termine a frase com ponto final.
- **Particípio**: Utilize sempre o particípio passado (ex: corrigido, adicionado, refatorado).
- **Corpo da Mensagem**: Se necessário, adicione um corpo à mensagem separado por uma linha em branco, descrevendo detalhes adicionais em bullets.
