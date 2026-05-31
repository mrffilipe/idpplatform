using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kyvo.API.Swagger;

/// <summary>
/// Documents standard ProblemDetails error responses produced by <see cref="Middlewares.ApplicationExceptionMiddleware"/>.
/// </summary>
public sealed class StandardErrorResponsesOperationFilter : IOperationFilter
{
    private static readonly (int StatusCode, string Description)[] StandardErrors =
    [
        (StatusCodes.Status400BadRequest, "Validation or malformed request."),
        (StatusCodes.Status401Unauthorized, "Missing or invalid authentication."),
        (StatusCodes.Status403Forbidden, "Authenticated but not allowed to perform this action."),
        (StatusCodes.Status404NotFound, "Resource was not found."),
        (StatusCodes.Status409Conflict, "Business rule conflict."),
        (StatusCodes.Status500InternalServerError, "Unexpected server error.")
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.GroupName == "oidc")
        {
            return;
        }

        foreach (var (statusCode, description) in StandardErrors)
        {
            var key = statusCode.ToString();
            if (operation.Responses.ContainsKey(key))
            {
                continue;
            }

            operation.Responses.Add(key, new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository)
                    }
                }
            });
        }
    }
}
