namespace PensaComigo.Application.Storage;

/// <summary>
/// Seam de armazenamento (mesma jogada da Fatia 10): a Application sabe que existe "algo
/// que assina uma URL de upload", não que esse algo é o Supabase. Impl no host, fake no teste.
/// </summary>
public interface IStorage
{
    /// <summary>Devolve a URL assinada (absoluta) para o cliente subir o arquivo direto no storage.</summary>
    Task<string> GerarUrlUploadAssinadaAsync(string path, CancellationToken ct);
}
