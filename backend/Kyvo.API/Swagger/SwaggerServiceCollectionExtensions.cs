using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Kyvo.API.Swagger;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Kyvo REST API",
                Version = "v1",
                Description =
                    "Versioned REST API for tenants, applications, memberships, identity providers, and audit logs."
            });

            options.SwaggerDoc("oidc", new OpenApiInfo
            {
                Title = "OAuth 2.0, OpenID Connect & Account",
                Version = "1.0",
                Description =
                    "Browser login forms under /account, OAuth authorization and token endpoints under /connect, " +
                    "and OpenID Provider metadata under /.well-known."
            });

            options.DocInclusionPredicate((docName, apiDesc) =>
                string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.OperationFilter<StandardErrorResponsesOperationFilter>();
            options.OperationFilter<OidcErrorResponsesOperationFilter>();
            options.OperationFilter<OidcAuthorizeOperationFilter>();
        });

        return services;
    }
}
