# 0011 — Fatia 13: Gridify como padrão de listagem

**Data:** 2026-08-02 · **Aula:** `lessons/0013-gridify-listagem-paginada.html` · **Issues:** 01 (follow-up infra), 03 (conformar Tags)

## Contexto
Decisão project-wide (arquitetura §7.1 / Decisão #19): toda listagem usa Gridify e responde
no envelope `{ items, totalItems }`. Tags já estava implementada com lista crua → foi conformada.

## O que foi aprendido
- **`GridifyQuery` como base da Query CQRS** dá `Page/PageSize/OrderBy/Filter` bindados da
  querystring e visíveis no Swagger sem código extra.
- **`record` não herda de classe comum** → `ListarTagsQuery` virou `class`. Detalhe de C# que
  força mudança de forma quando se herda de tipo de biblioteca.
- **`GridifyMapper` é whitelist**, não mapeamento de saída. Sem ele o cliente filtraria por
  qualquer propriedade da entidade.
- **`OrderBy` padrão é requisito de correção**, não estética: sem `ORDER BY` o Postgres não
  garante ordem estável entre páginas.
- **Colisão de nome:** o pacote Gridify já exporta `Paging<T>` → `CS0104`. Envelope próprio
  ficou `Pagina<T>` (pt-br resolve).

## Decisões
- **NuGet (`Gridify` 2.19.1 + `Gridify.EntityFramework`)**, não a cópia vendorada do
  `service-escolaweb`. O único patch local relevante lá era `DefaultOrderBy` virtual —
  substituído por uma linha no repositório.
- **`Pagina<T>` no Domain** (`Common/Pagina.cs`): é o único projeto que Persistence e
  Application enxergam. Domain ganhou dependência do pacote `Gridify` (só pela abstração
  `IGridifyQuery` na assinatura do repositório).
- **Repositório devolve `Pagina<Tag>` materializada**, não `(IQueryable, TotalItems)` como o
  escolaweb — sem AutoMapper/`ProjectTo` aqui, expor `IQueryable` só levaria EF pra Application.

## A revisitar
- Quando surgir a listagem de **Posts** (issue 06), reavaliar: com muitos campos e joins pode
  valer `GenerateMappings()` + `AddMap` para campos derivados.
- Se aparecer projeção pesada, aí sim considerar devolver `IQueryable` + projeção no handler.
