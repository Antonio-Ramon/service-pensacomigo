namespace PensaComigo.Application.Common;

/// <summary>
/// Lista de termos proibidos em comentário. Função pura: entra texto, sai bool.
/// Mora aqui, e não no Shared como a spec dizia, pela mesma razão do
/// <see cref="GeradorSlug"/> — a seta é Shared → Application, então o validator
/// nunca enxergaria de volta.
/// </summary>
public static class FiltroPalavrao
{
    // Comparação é feita sobre o texto já normalizado (minúsculo, sem acento),
    // então a lista também vive normalizada.
    private static readonly HashSet<string> Proibidas =
    [
        "merda", "porra", "caralho", "foda", "fodase", "buceta", "cu", "cuzao",
        "puta", "putaqueopariu", "viado", "bicha", "corno", "arrombado",
        "desgraca", "filhadaputa", "filhodaputa", "otario", "babaca", "idiota",
        "imbecil", "retardado", "vagabunda", "piranha", "escroto", "bosta",
    ];

    /// <summary>
    /// Reusa o <see cref="GeradorSlug.Gerar"/>: ele já tira acento, baixa a caixa e
    /// troca pontuação por hífen — sobra a lista de palavras pra conferir uma a uma.
    /// Palavra inteira, não substring: "cu" não pode reprovar "curso".
    /// </summary>
    public static bool Contem(string? texto) =>
        !string.IsNullOrWhiteSpace(texto)
        && GeradorSlug.Gerar(texto)
                      .Split('-', StringSplitOptions.RemoveEmptyEntries)
                      .Any(Proibidas.Contains);
}
