using PensaComigo.Application.Messaging;
using PensaComigo.Application.Posts;
using PensaComigo.Domain.Common;

namespace PensaComigo.Application.Autores;

/// <summary>"Quem escreve" da home (issue #22). Leitura pura, pública, sem paginação real:
/// são 2 autores — o envelope Pagina fica só pela consistência do contrato.</summary>
public record ListarAutoresQuery : IQuery<Pagina<AutorResponse>>;
