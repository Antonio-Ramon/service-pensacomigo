using MediatR;

namespace PensaComigo.Application.Messaging;

/// <summary>
/// Buffer de eventos da requisição (Scoped). O handler enfileira; quem publica é o
/// <c>DespachoDeEventosBehavior</c>, depois do commit — o corpo do handler é todo pré-commit
/// (ADR 0001).
/// </summary>
public class FilaDeEventos
{
    private readonly List<INotification> eventos = [];

    public void Adicionar(INotification evento) => eventos.Add(evento);

    public IReadOnlyList<INotification> Drenar()
    {
        var copia = eventos.ToArray();
        eventos.Clear();
        return copia;
    }
}
