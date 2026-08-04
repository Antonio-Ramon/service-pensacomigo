namespace PensaComigo.Application.Imagens;

/// <summary>
/// Whitelist única de imagem: extensão → content-type. O content-type do upload sai DAQUI,
/// não do que o cliente declarou (um .png com <c>Content-Type: text/html</c> servido de volta
/// pelo storage vira XSS).
/// </summary>
public static class ImagensPermitidas
{
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB, igual ao limite do bucket

    public static readonly Dictionary<string, string> Tipos = new()
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    };
}
