using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Posts.Obter;

/// <summary>
/// Detalhe por id para o EDITOR do autor (issue #29): leitura pura, sem incrementar
/// visualizações — abrir o próprio post pra editar não é uma leitura do público.
/// </summary>
public record ObterPostQuery(Guid Id, Guid AutorId) : IQuery<PostDetalheResponse>;
