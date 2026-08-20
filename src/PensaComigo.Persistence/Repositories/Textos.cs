using System.Globalization;
using System.Text;

namespace PensaComigo.Persistence.Repositories;

internal static class Textos
{
    /// <summary>Espelho em C# do unaccent() do Postgres: normaliza o TERMO buscado,
    /// enquanto a coluna é normalizada no SQL — os dois lados sem acento se encontram.</summary>
    public static string RemoverAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
