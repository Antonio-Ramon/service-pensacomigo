using System.Net;
using System.Net.Sockets;

namespace PensaComigo.Application.Common;

/// <summary>
/// Guarda de SSRF do preview de link (issue #21): o servidor faz request pra URL escolhida
/// pelo usuário, então IP privado/loopback/link-local é barrado — em TODO salto de redirect.
/// </summary>
public static class GuardaSsrf
{
    public static bool EnderecoPermitido(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();

        if (IPAddress.IsLoopback(ip)) return false;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            return !(ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal
                     || ip.Equals(IPAddress.IPv6Any));

        var b = ip.GetAddressBytes();
        return !(b[0] == 0                                  // 0.0.0.0/8
                 || b[0] == 10                              // 10/8
                 || b[0] == 127                             // 127/8
                 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)   // 172.16/12
                 || (b[0] == 192 && b[1] == 168)            // 192.168/16
                 || (b[0] == 169 && b[1] == 254));          // 169.254/16 (link-local/metadata)
    }
}
