using System.Net;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Domain.Exceptions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        catch (AntiforgeryValidationException)
        {
            if (await TryRedirectAccountLoginAsync(context, "session_expired").ConfigureAwait(false))
            {
                return;
            }

            await WriteProblemAsync(
                context,
                HttpStatusCode.BadRequest,
                ApiErrorMessages.DomainValidationTitle,
                ApiErrorMessages.Account.SessionExpiredRetryLogin);
        }
        catch (BadHttpRequestException ex) when (IsAntiforgeryFailure(ex))
        {
            if (await TryRedirectAccountLoginAsync(context, "session_expired").ConfigureAwait(false))
            {
                return;
            }

            await WriteProblemAsync(
                context,
                HttpStatusCode.BadRequest,
                ApiErrorMessages.DomainValidationTitle,
                ApiErrorMessages.Account.SessionExpiredRetryLogin);
        }
        catch (Exception ex)
        {
            var isDevelopment = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true;
            var detail = isDevelopment
                ? FormatDevelopmentExceptionDetail(ex)
                : ApiErrorMessages.UnexpectedErrorDetail;

            await WriteProblemAsync(
                context,
                HttpStatusCode.InternalServerError,
                ApiErrorMessages.UnhandledServerErrorTitle,
                detail);
        }
    }

    private static bool IsAntiforgeryFailure(BadHttpRequestException ex) =>
        ex.Message.Contains("antiforgery", StringComparison.OrdinalIgnoreCase);

    private static Task<bool> TryRedirectAccountLoginAsync(HttpContext context, string errorCode)
    {
        if (!context.Request.Path.StartsWithSegments("/account", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(false);
        }

        var returnUrl = context.Request.Query["returnUrl"].ToString();
        if (string.IsNullOrWhiteSpace(returnUrl)
            && context.Request.HasFormContentType
            && context.Request.Form.TryGetValue("returnUrl", out var formReturnUrl))
        {
            returnUrl = formReturnUrl.ToString();
        }

        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["returnUrl"] = string.IsNullOrWhiteSpace(returnUrl) ? null : returnUrl,
            ["error"] = errorCode
        }.Where(p => !string.IsNullOrWhiteSpace(p.Value)));

        context.Response.Redirect($"/account/login{query}");
        return Task.FromResult(true);
    }

    private static string FormatDevelopmentExceptionDetail(Exception ex)
    {
        var parts = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException)
        {
            parts.Add(current.Message);
        }

        return string.Join(" -> ", parts);
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
