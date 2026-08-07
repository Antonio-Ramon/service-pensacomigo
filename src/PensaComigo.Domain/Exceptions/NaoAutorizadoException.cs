namespace PensaComigo.Domain.Exceptions;

/// <summary>Credencial ausente, inválida ou expirada → 401. Diferente de "não é seu"
/// (esse é 404 de propósito, para não confirmar a existência do recurso).</summary>
public class NaoAutorizadoException(string mensagem) : Exception(mensagem);
