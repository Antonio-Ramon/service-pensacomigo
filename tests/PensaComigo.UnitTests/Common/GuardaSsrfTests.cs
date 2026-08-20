using System.Net;
using PensaComigo.Application.Common;

namespace PensaComigo.UnitTests.Common;

public class GuardaSsrfTests
{
    [Theory]
    [InlineData("127.0.0.1")]       // loopback
    [InlineData("10.1.2.3")]        // 10/8
    [InlineData("172.16.0.1")]      // borda de baixo do 172.16/12
    [InlineData("172.31.255.255")]  // borda de cima do 172.16/12
    [InlineData("192.168.0.10")]    // 192.168/16
    [InlineData("169.254.169.254")] // link-local (metadata de cloud)
    [InlineData("0.0.0.0")]
    [InlineData("::1")]             // loopback v6
    [InlineData("fe80::1")]         // link-local v6
    [InlineData("fd00::1")]         // unique-local v6
    [InlineData("::ffff:127.0.0.1")] // v4 mapeado em v6 não escapa da guarda
    public void Endereco_interno_e_bloqueado(string ip) =>
        Assert.False(GuardaSsrf.EnderecoPermitido(IPAddress.Parse(ip)));

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("172.32.0.1")]      // logo acima do range privado
    [InlineData("2606:4700::1111")] // v6 público
    public void Endereco_publico_passa(string ip) =>
        Assert.True(GuardaSsrf.EnderecoPermitido(IPAddress.Parse(ip)));
}
