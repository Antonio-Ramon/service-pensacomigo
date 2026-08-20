namespace PensaComigo.Domain.Enums;

/// <summary>Ciclo editorial do post: nasce rascunho, vai ao ar quando o autor publica.</summary>
public enum StatusPost
{
    Rascunho = 0,
    Publicado = 1,

    /// <summary>Publicação futura: entra no ar quando <c>DataPublicacao</c> vence —
    /// resolvido na consulta do feed, sem job de fundo.</summary>
    Agendado = 2,
}
