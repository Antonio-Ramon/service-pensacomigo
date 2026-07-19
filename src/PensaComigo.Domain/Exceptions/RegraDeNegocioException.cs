namespace PensaComigo.Domain.Exceptions;

/// <summary>Invariante de negócio violada. Mapeada a HTTP 422 pelo GlobalExceptionHandler.</summary>
public sealed class RegraDeNegocioException(string mensagem) : Exception(mensagem);
