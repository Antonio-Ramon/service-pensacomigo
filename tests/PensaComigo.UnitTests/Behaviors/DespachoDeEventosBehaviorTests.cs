using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PensaComigo.Application;
using PensaComigo.Application.Behaviors;
using PensaComigo.Application.Messaging;
using PensaComigo.Domain.Repositories;

namespace PensaComigo.UnitTests.Behaviors;

/// <summary>
/// Trava a regra do ADR 0001: evento só sai depois do commit. Inverter a ordem de registro
/// dos behaviors no <c>AddApplication</c> compila e sobe — este teste é o único sinal.
/// </summary>
public class DespachoDeEventosBehaviorTests
{
    private record Evento : INotification;

    private record Comando : ICommand<string>;

    private sealed class PublisherFake : IPublisher
    {
        public List<INotification> Publicados { get; } = [];

        public Task Publish(object notification, CancellationToken ct = default) =>
            Publish((INotification)notification, ct);

        public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
            where TNotification : INotification
        {
            Publicados.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkFake(bool estoura) : IUnitOfWork
    {
        public Task<int> CommitAsync(CancellationToken ct = default) =>
            estoura ? throw new InvalidOperationException("commit falhou") : Task.FromResult(1);
    }

    /// <summary>Monta a cebola na MESMA ordem do AddApplication: Despacho por fora, UnitOfWork
    /// por dentro, handler no centro enfileirando.</summary>
    private static Task<string> Executar(FilaDeEventos fila, PublisherFake publisher, bool commitEstoura)
    {
        var despacho = new DespachoDeEventosBehavior<Comando, string>(fila, publisher);
        var unitOfWork = new UnitOfWorkBehavior<Comando, string>(new UnitOfWorkFake(commitEstoura));

        return despacho.Handle(
            new Comando(),
            _ => unitOfWork.Handle(new Comando(), _ =>
            {
                fila.Adicionar(new Evento());
                return Task.FromResult("ok");
            }, default),
            default);
    }

    [Fact]
    public async Task Commit_bem_sucedido_publica_o_evento()
    {
        var fila = new FilaDeEventos();
        var publisher = new PublisherFake();

        var resposta = await Executar(fila, publisher, commitEstoura: false);

        Assert.Equal("ok", resposta);
        Assert.Single(publisher.Publicados);
    }

    [Fact]
    public async Task Commit_que_estoura_nao_publica_nada()
    {
        var fila = new FilaDeEventos();
        var publisher = new PublisherFake();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Executar(fila, publisher, commitEstoura: true));

        Assert.Empty(publisher.Publicados);
    }

    /// <summary>Os dois testes acima montam a cebola à mão; este confere que o AddApplication
    /// monta na mesma ordem — sem ele, inverter as duas linhas do registro passaria batido.</summary>
    [Fact]
    public void Despacho_e_registrado_por_fora_do_unit_of_work()
    {
        var behaviors = new ServiceCollection().AddApplication()
            .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
            .Select(d => d.ImplementationType)
            .ToList();

        Assert.True(
            behaviors.IndexOf(typeof(DespachoDeEventosBehavior<,>))
            < behaviors.IndexOf(typeof(UnitOfWorkBehavior<,>)),
            "DespachoDeEventosBehavior precisa ser registrado ANTES do UnitOfWorkBehavior para envolvê-lo.");
    }

    [Fact]
    public void Drenar_esvazia_a_fila()
    {
        var fila = new FilaDeEventos();
        fila.Adicionar(new Evento());

        Assert.Single(fila.Drenar());
        Assert.Empty(fila.Drenar());
    }
}
