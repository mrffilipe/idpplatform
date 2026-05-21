using System.Net;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Middlewares;

public sealed class ApplicationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ApplicationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedApplicationException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Unauthorized,
                ApiErrorMessages.UnauthorizedTitle,
                ex.Message);
        }
        catch (ForbiddenApplicationException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Forbidden,
                ApiErrorMessages.ForbiddenTitle,
                ex.Message);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.BadRequest,
                ApiErrorMessages.DomainValidationTitle,
                ex.Message);
        }
        catch (InvalidClientException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Unauthorized,
                ApiErrorMessages.InvalidClientTitle,
                ex.Message);
        }
        catch (DomainBusinessRuleException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.Conflict,
                ApiErrorMessages.DomainBusinessRuleTitle,
                ex.Message);
        }
        catch (DomainNotFoundException ex)
        {
            await WriteProblemAsync(
                context,
                HttpStatusCode.NotFound,
                ApiErrorMessages.NotFoundTitle,
                ex.Message);
        }
        catch (Exception ex)
        {
            var detail = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                ? ex.Message
                : ApiErrorMessages.UnexpectedErrorDetail;

            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                ApiErrorMessages.UnhandledServerErrorTitle,
                detail);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode code,
        string title,
        string detail)
    {
        var problem = new ProblemDetails
        {
            Status = (int)code,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = (int)code;
        context.Response.ContentType = ApiErrorMessages.ProblemJsonContentType;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
