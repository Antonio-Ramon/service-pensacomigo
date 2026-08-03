using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PensaComigo.Application.Common;

/// <summary>
/// Deriva slug de um texto livre e resolve colisão com sufixo numérico.
/// Função pura: sem I/O, sem DI — quem consulta o banco é o handler.
/// </summary>
public static partial class GeradorSlug
{
    /// <summary>"Saúde Mental!" → "saude-mental". Tira acento, minúscula, não-alfanumérico vira hífen.</summary>
    public static string Gerar(string texto)
    {
        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var semAcento = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(c);

        return NaoAlfanumerico().Replace(semAcento.ToString(), "-").Trim('-');
    }

    /// <summary>
    /// Primeiro slug livre: "post", "post-2", "post-3"... <paramref name="ocupados"/> são os
    /// slugs já existentes (o handler busca uma vez, por prefixo, antes de chamar).
    /// </summary>
    public static string ResolverColisao(string slugBase, IEnumerable<string> ocupados)
    {
        var set = ocupados as HashSet<string> ?? [.. ocupados];
        if (!set.Contains(slugBase)) return slugBase;

        for (var n = 2; ; n++)
        {
            var candidato = $"{slugBase}-{n}";
            if (!set.Contains(candidato)) return candidato;
        }
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NaoAlfanumerico();
}
