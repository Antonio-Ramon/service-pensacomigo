namespace PensaComigo.Application.Comentarios.Listar;

/// <summary>
/// Shape da leitura: só o que o leitor vê, sem <c>PostId</c> (já está na rota).
/// <c>Aprovado</c> é sempre <c>true</c> para o leitor — só o admin recebe ocultos,
/// e é por esse campo que a tela de moderação sabe quem reexibir.
/// <para>
/// A árvore é RASA de propósito (regra #7, 1 nível): a raiz carrega suas respostas
/// e acabou — não existe <c>Respostas</c> dentro de <see cref="RespostaResponse"/>.
/// O tipo espelha a regra de negócio; um shape recursivo prometeria o que a API não faz.
/// </para>
/// <para>
/// <c>AutorImagemUrl</c> e <c>EhAutorDoPost</c> só têm valor em comentário de usuário
/// logado; para o visitante anônimo são <c>null</c>/<c>false</c>. A comparação com o
/// dono do post é feita AQUI e não no cliente: o front não recebe o id de quem comentou.
/// </para>
/// </summary>
public record ComentarioListaResponse(
    Guid Id,
    string Autor,
    string Conteudo,
    DateTime DataCriacao,
    bool Aprovado,
    string? AutorImagemUrl,
    bool EhAutorDoPost,
    IReadOnlyList<RespostaResponse> Respostas);

public record RespostaResponse(
    Guid Id,
    string Autor,
    string Conteudo,
    DateTime DataCriacao,
    bool Aprovado,
    string? AutorImagemUrl,
    bool EhAutorDoPost);
