using System.Text.Json.Serialization;

namespace PensaComigo.Shared.Erros;

/// <summary>Um problema, amarrado ao campo que o causou. <c>Key</c> é o nome do campo
/// no JSON enviado (camelCase); quando o erro não é de campo, é a origem ("Erro").</summary>
public record Notificacao(string Key, string Message);

/// <summary>
/// Corpo único de TODA resposta de erro da API ,
/// para o front tratar os dois serviços com um adaptador só. Sucesso continua saindo cru
/// (sem envelope): o status HTTP já diz que deu certo.
/// </summary>
/// <remarks>Nomes em inglês de propósito: são o contrato de fio, e precisam bater
/// caractere a caractere com o escolaweb (<c>successed</c>, não <c>succeeded</c>).</remarks>
public class RespostaErro
{
    public bool Successed { get; init; }

    public required string Message { get; init; }

    public IReadOnlyList<Notificacao> Notifications { get; init; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Debug { get; init; }
}
