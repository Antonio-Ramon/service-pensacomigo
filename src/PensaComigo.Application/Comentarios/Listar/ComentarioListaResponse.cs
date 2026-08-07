namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>
/// Shape da leitura pública: só o que o leitor vê. Sem <c>Aprovado</c> (tudo que sai
/// daqui é aprovado por definição) e sem <c>PostId</c> (já está na rota).
/// <para>
/// A árvore é RASA de propósito (regra #7, 1 nível): a raiz carrega suas respostas
/// e acabou — não existe <c>Respostas</c> dentro de <see cref="RespostaResponse"/>.
/// O tipo espelha a regra de negócio; um shape recursivo prometeria o que a API não faz.
/// </para>
/// </summary>
public record ComentarioListaResponse(
    Guid Id,
    string Autor,
    string Conteudo,
    DateTime DataCriacao,
    IReadOnlyList<RespostaResponse> Respostas);

public record RespostaResponse(Guid Id, string Autor, string Conteudo, DateTime DataCriacao);
