namespace PensaComigo.Application.Storage;

/// <summary>
/// Seam de armazenamento (mesma jogada da Fatia 10): a Application sabe que existe "algo
/// que guarda bytes e devolve uma URL", não que esse algo é o Supabase. Impl no host, fake no teste.
/// </summary>
public interface IStorage
{
    /// <summary>Sobe o conteúdo em <paramref name="path"/> e devolve a URL pública de leitura.</summary>
    Task<string> EnviarAsync(string path, Stream conteudo, string contentType, CancellationToken ct);
}
