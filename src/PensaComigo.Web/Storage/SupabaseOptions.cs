using System.ComponentModel.DataAnnotations;

namespace PensaComigo.Web.Storage;

/// <summary>
/// Seção "Supabase" do appsettings, tipada (options pattern). Validada no start:
/// config faltando derruba a subida, não o primeiro upload de madrugada.
/// </summary>
public class SupabaseOptions
{
    /// <summary>Ex.: <c>https://xxxx.supabase.co</c> (sem barra no fim).</summary>
    [Required]
    public string Url { get; set; } = "";

    /// <summary>Chave de admin: ignora RLS. Vem de user-secrets/env, nunca do appsettings comitado.</summary>
    [Required]
    public string ServiceRoleKey { get; set; } = "";

    public string Bucket { get; set; } = "imagens";
}
