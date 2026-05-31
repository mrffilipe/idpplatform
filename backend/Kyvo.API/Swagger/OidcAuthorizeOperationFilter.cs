using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kyvo.API.Swagger;

/// <summary>
/// Documents standard OAuth 2.0 / OIDC query parameters on GET /connect/authorize.
/// </summary>
public sealed class OidcAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.ApiDescription.RelativePath?.Contains("connect/authorize", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        AddQuery(operation, "client_id", "Registered OAuth client identifier.", required: true);
        AddQuery(operation, "redirect_uri", "Must match a redirect URI registered for the client.", required: true);
        AddQuery(operation, "response_type", "Must be \"code\" (authorization code flow).", required: true);
        AddQuery(operation, "scope", "Space-delimited scopes (e.g. openid profile email offline_access).", required: true);
        AddQuery(operation, "state", "Opaque value echoed back on redirect.");
        AddQuery(operation, "prompt", "login, none, or combinations per OIDC.");
        AddQuery(operation, "max_age", "Maximum authentication age in seconds.");
        AddQuery(operation, "code_challenge", "PKCE code challenge (S256).");
        AddQuery(operation, "code_challenge_method", "PKCE method; must be S256 when challenge is sent.");
        AddQuery(operation, "nonce", "Replay protection for the ID token.");
    }

    private static void AddQuery(OpenApiOperation operation, string name, string description, bool required = false)
    {
        if (operation.Parameters.Any(p => p.Name == name && p.In == ParameterLocation.Query))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = required,
            Description = description,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
