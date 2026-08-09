using System.ComponentModel.DataAnnotations;

namespace PensaComigo.Web.Visitantes;

/// <summary>Seção <c>Visitantes</c> do config. Validada na subida (ver Program.cs).</summary>
public sealed class VisitantesOptions
{
    /// <summary>
    /// Segredo do HMAC que identifica o leitor anônimo. Nunca no appsettings — user-secrets em
    /// dev, variável de ambiente em prod. Trocar o valor invalida os <c>viewer_hash</c> gravados.
    /// </summary>
    [Required]
    [MinLength(32)]
    public string Pepper { get; set; } = "";
}
