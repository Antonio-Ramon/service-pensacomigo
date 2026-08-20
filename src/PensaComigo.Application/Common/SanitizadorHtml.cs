using Ganss.Xss;
using PensaComigo.Domain.Enums;
using PensaComigo.Domain.ValueObjects;

namespace PensaComigo.Application.Common;

/// <summary>
/// Fronteira de confiança do HTML dos blocos Texto (issue #18). Whitelist espelha o editor
/// Tiptap e o CSS público do front — a fonte canônica é o CONTEXT.md do front-pensacomigo:
/// mudou lá, muda aqui junto. Markup fora da lista é REMOVIDO, nunca rejeitado (salvar não falha).
/// </summary>
public static class SanitizadorHtml
{
    private static readonly HtmlSanitizer Sanitizer = Criar();

    // Tags cujo CONTEÚDO também é lixo: manter o texto de dentro vazaria o corpo do script.
    private static readonly HashSet<string> RemoverComConteudo = ["script", "style", "iframe", "svg", "math", "object", "embed"];

    private static HtmlSanitizer Criar()
    {
        // Construtor padrão de propósito: preserva UriAttributes (href é checado como URI).
        // O construtor via options zera essa lista e o esquema javascript: passaria.
        var s = new HtmlSanitizer();

        s.AllowedTags.Clear();
        s.AllowedTags.UnionWith([
            // estrutura
            "p", "h2", "h3", "blockquote", "ul", "ol", "li", "hr",
            // inline
            "strong", "em", "u", "s", "code", "mark", "a", "sup", "sub", "br",
            // padrões do design system (verse/aside)
            "div", "q", "b",
        ]);

        s.AllowedAttributes.Clear();
        s.AllowedAttributes.UnionWith(["href", "title", "class"]);

        s.AllowedSchemes.Clear();
        s.AllowedSchemes.UnionWith(["http", "https"]);

        s.AllowedClasses.Clear();
        s.AllowedClasses.UnionWith(["verse", "r", "aside"]);

        s.AllowedCssProperties.Clear();
        s.AllowedAtRules.Clear();

        // Tag fora da lista some, mas o texto de dentro sobrevive (<span>x</span> → x)...
        s.KeepChildNodes = true;
        // ...exceto nas tags perigosas, que somem com conteúdo e tudo.
        s.RemovingTag += (_, e) =>
        {
            if (RemoverComConteudo.Contains(e.Tag.TagName.ToLowerInvariant()))
                e.Tag.InnerHtml = string.Empty;
        };
        return s;
    }

    public static string Sanitizar(string html) => Sanitizer.Sanitize(html);

    /// <summary>Sanitiza o Html de cada bloco Texto, in-place. Imagem/link não carregam HTML.</summary>
    public static void SanitizarBlocos(IEnumerable<Bloco> blocos)
    {
        foreach (var b in blocos)
            if (b.Tipo == TipoBloco.Texto && b.Html is not null)
                b.Html = Sanitizar(b.Html);
    }
}
