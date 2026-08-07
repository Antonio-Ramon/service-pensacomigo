namespace PensaComigo.Application.Comentarios;

/// <summary>
/// Sem <c>DataCriacao</c> de propósito: a coluna tem <c>default now()</c> e quem
/// commita é o UnitOfWorkBehavior, DEPOIS do handler — aqui o valor ainda seria
/// <c>default(DateTime)</c>. A data sai na listagem, que lê do banco.
/// </summary>
public record ComentarioResponse(Guid Id, Guid PostId, Guid? ParentId, string Autor, string Conteudo, bool Aprovado);
