namespace PensaComigo.Domain.Exceptions;

/// <summary>Cliente passou do limite de chamadas na janela. Mapeada a HTTP 429
/// pelo GlobalExceptionHandler — o 429 que ficou de fora da Fatia 6.</summary>
public sealed class MuitasRequisicoesException(string mensagem) : Exception(mensagem);
