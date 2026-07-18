# Notas

## Preferências
- Vem de outra linguagem — pode pular explicação de conceitos universais (loop, if),
  focar no que é **específico de C#/.NET** e das arquiteturas.
- Concisão > gramática na comunicação.
- Quer prática ativa: implementar junto, não só ler código pronto.

## Progresso
- [x] Fatia 1 — Mapeamento EF Core + DbContext (aula 0001) — build verde
- [x] Fatia 2 — jsonb via HasConversion + ValueComparer (aula 0002). Código já
  estava no PostConfiguration desde a Fatia 1; esta fatia foi entendimento.
- [x] Fatia 3 — Repository pattern + DI (aula 0003). Interfaces no Domain, 4 impls
  esqueleto no Persistence, `AddPersistence()` no Program.cs. Build verde.
  DbContext ainda fora da DI (vem na Fatia 4).
- [x] Fatia 4 — Migration inicial + seed + AddDbContext (aula 0004). DbContext na DI,
  seed HasData (Antonio/Jéssica), migration `InicialSchema` gerada e conferida.
  Falta só `database update` contra Supabase real (passo de infra do usuário).
- [x] Fatia 5 — Pipeline MediatR (aula 0005). 3 behaviors (Logging/Validation/UnitOfWork),
  marcadores CQRS (`ICommand`/`IQuery`/`IBaseCommand`), ponte `IUnitOfWork` (DbContext).
  `AddApplication()` no Program. Build verde. Sem Command/validator real ainda — é o trilho.

## Cuidado ao montar quiz
- `data-a` é índice 0-based do botão correto. Já saiu errado 2x na aula 05 (embaralhei a
  posição da resposta mas não atualizei o índice). SEMPRE reconferir: contar os botões de 0 e
  bater com o `data-a` antes de entregar.

## Observações técnicas encontradas
- `Usuario.IsAdmin` na entidade tem default `true`, mas schema/ticket pede coluna
  default `false`. Corrigido no default da COLUNA (config). Vale conferir o default
  da propriedade C# depois (seed cria admins explicitamente).
