using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Imagens.Enviar;

/// <summary>
/// Sobe uma imagem pelo backend. <c>ICommand</c> porque tem efeito colateral (grava bytes),
/// mesmo que nada mude no nosso Postgres — o UnitOfWork commita um conjunto vazio, no-op.
/// <paramref name="Conteudo"/> é <c>Stream</c>, não <c>byte[]</c>: 5 MB por request na memória
/// é o tipo de coisa que só aparece quando 20 autores publicam juntos.
/// </summary>
public record EnviarImagemCommand(
    Guid UsuarioId,
    string NomeArquivo,
    long Tamanho,
    Stream Conteudo) : ICommand<ImagemResponse>;
