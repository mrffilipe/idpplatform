using Kyvo.API.Models.Oidc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kyvo.API.Swagger;

/// <summary>
/// Documents OAuth 2.0 JSON error bodies (RFC 6749) on OIDC/account operations.
/// </summary>
public sealed class OidcErrorResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.GroupName != "oidc")
        {
            return;
        }

        if (operation.Responses.ContainsKey("400"))
        {
            return;
        }

        operation.Responses.Add("400", new OpenApiResponse
        {
            Description = "OAuth error (invalid_request, invalid_client, invalid_grant, etc.).",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(typeof(OidcErrorJsonResponse), context.SchemaRepository)
                }
            }
        });
    }
}
