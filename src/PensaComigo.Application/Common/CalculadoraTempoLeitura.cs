using System.Text.RegularExpressions;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Common;

/// <summary>
/// Tempo de leitura em minutos, calculado na Application ao salvar o post
/// (campo calculado no handler, não na entidade — ver CLAUDE.md).
/// </summary>
public static partial class CalculadoraTempoLeitura
{
    // ponytail: 200 ppm é a média usada por Medium/estudos de leitura silenciosa;
    // vira config se algum dia alguém reclamar do número.
    private const int PalavrasPorMinuto = 200;

    public static int Calcular(IEnumerable<Bloco> blocos)
    {
        var palavras = blocos
            .Where(b => b.Tipo == TipoBloco.Texto && !string.IsNullOrWhiteSpace(b.Html))
            .Sum(b => ContarPalavras(b.Html!));

        // Post só de imagem ainda leva alguns segundos: mínimo de 1 minuto.
        return Math.Max(1, (int)Math.Ceiling(palavras / (double)PalavrasPorMinuto));
    }

    private static int ContarPalavras(string html) =>
        Tags().Replace(html, " ")
              .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tags();
}
