using PensaComigo.Application.Messaging;

namespace PensaComigo.Application.Links.Preview;

/// <summary>Leitura pura (não persiste nada) → Query, sem commit.</summary>
public record ObterPreviewLinkQuery(string Url) : IQuery<LinkPreviewResponse>;
