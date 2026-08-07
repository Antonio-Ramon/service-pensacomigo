namespace PensaComigo.Application.Imagens.Enviar;

/// <summary>
/// <paramref name="Path"/> é o que o autor guarda no bloco de imagem / capa do post;
/// <paramref name="Url"/> é a leitura pública, para exibir na hora.
/// </summary>
public record ImagemResponse(string Path, string Url);
