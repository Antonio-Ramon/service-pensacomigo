namespace PensaComigo.Application.Common;

/// <summary>
/// Aritmética do rate limit, sem cache e sem relógio: recebe os carimbos anteriores
/// e o "agora", devolve a decisão. Todo o estado é do chamador — por isso dá pra
/// testar em unit sem esperar um minuto passar.
/// </summary>
public static class JanelaDeslizante
{
    /// <summary>
    /// Carimbos ainda dentro da janela, acrescidos de <paramref name="agora"/>.
    /// <c>null</c> quando o limite já estourou (o chamador traduz em 429).
    /// </summary>
    public static List<DateTime>? Registrar(
        IEnumerable<DateTime> carimbos, DateTime agora, TimeSpan janela, int maximo)
    {
        // Deslizante, não fixa: a janela anda com o relógio em vez de zerar de minuto
        // em minuto — senão dá pra mandar 5 às 12:00:59 e mais 5 às 12:01:00.
        var vigentes = carimbos.Where(c => agora - c < janela).ToList();

        if (vigentes.Count >= maximo) return null;

        vigentes.Add(agora);
        return vigentes;
    }
}
