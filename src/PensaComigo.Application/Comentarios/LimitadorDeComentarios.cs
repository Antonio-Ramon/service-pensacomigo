using Microsoft.Extensions.Caching.Memory;
using PensaComigo.Application.Common;
using PensaComigo.Domain.Exceptions;

namespace PensaComigo.Application.Comentarios;

/// <summary>
/// Cola entre o <see cref="JanelaDeslizante"/> (pura) e o estado real. Classe concreta,
/// sem interface: só existe uma implementação e o teste de integração exercita esta mesma.
/// </summary>
/// <remarks>
/// ponytail: <c>IMemoryCache</c> é memória DO PROCESSO — com duas instâncias da API cada
/// uma conta seu próprio balde e o limite efetivo dobra. Trocar por Redis
/// (<c>IDistributedCache</c>) quando houver mais de uma instância.
/// </remarks>
public sealed class LimitadorDeComentarios(IMemoryCache cache)
{
    public const int Maximo = 5;
    public static readonly TimeSpan Janela = TimeSpan.FromMinutes(1);

    /// <summary>Contabiliza mais um comentário do visitante, ou estoura 429.</summary>
    public void Registrar(string visitante)
    {
        var chave = $"comentarios:{visitante}";

        var carimbos = JanelaDeslizante.Registrar(
                           cache.Get<List<DateTime>>(chave) ?? [],
                           DateTime.UtcNow, Janela, Maximo)
                       ?? throw new MuitasRequisicoesException(
                           $"Limite de {Maximo} comentários por minuto atingido. Aguarde um pouco.");

        // Expiração absoluta = a própria janela: visitante que parou de comentar
        // some do cache sozinho, sem rotina de limpeza.
        cache.Set(chave, carimbos, Janela);
    }
}
