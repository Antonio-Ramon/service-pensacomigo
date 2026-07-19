namespace PensaComigo.Domain.Exceptions;

/// <summary>Recurso pedido não existe. Mapeada a HTTP 404 pelo GlobalExceptionHandler.</summary>
public sealed class NaoEncontradoException(string recurso, object chave)
    : Exception($"{recurso} '{chave}' não encontrado.");
