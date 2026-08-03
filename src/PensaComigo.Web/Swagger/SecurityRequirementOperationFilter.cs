using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PensaComigo.Web.Swagger;

/// <summary>
/// Põe o cadeado do Swagger SÓ nas rotas que exigem token: olha os metadados do endpoint
/// (atributos do controller + da action) e ignora quem tem [AllowAnonymous].
///
/// É um DOCUMENT filter, não operation filter, de propósito: no Microsoft.OpenApi 2.x a
/// referência ao security scheme só serializa se souber o OpenApiDocument de destino
/// (`new OpenApiSecuritySchemeReference("Bearer", document)`); sem ele o JSON sai
/// `"security": [{}]` e o cadeado não aparece. Só o document filter recebe o documento.
/// </summary>
public class SecurityRequirementOperationFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (var api in context.ApiDescriptions)
        {
            var metadata = api.ActionDescriptor.EndpointMetadata;
            if (metadata.OfType<IAllowAnonymous>().Any()) continue;
            if (!metadata.OfType<IAuthorizeData>().Any()) continue;

            if (!document.Paths.TryGetValue("/" + api.RelativePath, out var path)) continue;
            if (api.HttpMethod is null) continue;
            if (!path.Operations!.TryGetValue(HttpMethod.Parse(api.HttpMethod), out var operation)) continue;

            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
                },
            ];
        }
    }
}
