# 07 — Comentários

**What to build:** Leitor comenta num post informando o nome (sem conta) e pode responder a um comentário (1 nível só). Moderação automática estilo YouTube: rate limit por visitante e filtro de palavrão — comentário limpo publica na hora. Leitor lista os comentários aprovados com respostas. Admin autenticado esconde ou deleta comentário publicado.

**Blocked by:** 05 — Post CRUD (post precisa existir), 02 — Auth (moderação exige admin).

**Status:** ready-for-agent

- [ ] Comentar com nome (anônimo); resposta 1 nível; 2º nível bloqueado no handler/validator (não no schema)
- [ ] Rate limit: 5 comentários por `viewer_hash` em janela deslizante de 1 min → 429; estado em cache de memória (`ponytail:` Redis) [unit: avaliação do rate limit]
- [ ] Filtro de palavrão (lista no Shared); match → erro de validação, nada sujo entra no banco [unit]
- [ ] Comentário limpo publica imediatamente (`aprovado = true`)
- [ ] Listar comentários aprovados de um post, com respostas (anônimo), via **Gridify**: `ListarComentariosQuery : GridifyQuery` (filtra sempre por `post_id` + `aprovado`), paginação/ordenação da querystring, resposta no envelope `Paging<T>`. Ver arquitetura §7.1 / Decisão #19.
- [ ] Admin (autenticado) esconde (`aprovado=false`) ou deleta comentário
- [ ] Teste de integração: fluxo comentar/responder/moderar; palavrão bloqueado; 6º comentário no minuto → 429
