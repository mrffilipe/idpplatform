using System.Security.Claims;
using System.Text.Json;
using Kyvo.API.Common;
using Kyvo.API.Models.Oidc;
using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.LocalAuthentication;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Kyvo.Infrastructure.Configurations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kyvo.API.Controllers;

/// <summary>
/// Browser-based sign-in and sign-out (form posts from Blazor login UI). Not part of the versioned JSON API.
/// </summary>
[ApiExplorerSettings(GroupName = "oidc")]
[Tags("Account (browser login)")]
[AllowAnonymous]
public sealed class AccountController : Controller
{
    private readonly ILocalAuthenticationService _localAuth;
    private readonly IExternalLoginService _externalLogin;
    private readonly IAuthSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;
    private readonly IAntiforgery _antiforgery;

    public AccountController(
        ILocalAuthenticationService localAuth,
        IExternalLoginService externalLogin,
        IAuthSessionRepository sessions,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions,
        IAntiforgery antiforgery)
    {
        _localAuth = localAuth;
        _externalLogin = externalLogin;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
        _antiforgery = antiforgery;
    }

    /// <summary>
    /// Local email/password sign-in; establishes an authentication cookie for the OAuth authorize flow.
    /// </summary>
    /// <remarks>
    /// On failure, redirects to <c>/account/login</c> with an <c>error</c> query code
    /// (<c>invalid_credentials</c>, <c>missing_fields</c>, <c>session_expired</c>, etc.).
    /// </remarks>
    [HttpPost("/account/signin")]
    [IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Login(
        [FromForm] AccountLoginForm form,
        CancellationToken cancellationToken)
    {
        var antiforgeryFailure = await TryRedirectOnAntiforgeryFailureAsync(form.ReturnUrl, cancellationToken);
        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        if (string.IsNullOrWhiteSpace(form.Email) || string.IsNullOrWhiteSpace(form.Password))
        {
            return RedirectToLogin(form.ReturnUrl, "missing_fields");
        }

        var login = await _localAuth.LoginAsync(
            new LocalLoginRequest { Email = form.Email, Password = form.Password },
            cancellationToken);

        if (login is null)
        {
            return RedirectToLogin(form.ReturnUrl, "invalid_credentials");
        }

        return await CompleteLoginAsync(login.ToExternalLoginResult(), form.ReturnUrl, cancellationToken);
    }

    /// <summary>
    /// Federated sign-in (e.g. Firebase id_token); establishes an authentication cookie.
    /// </summary>
    [HttpPost("/account/external-signin")]
    [IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> ExternalLogin(
        [FromForm] AccountExternalLoginForm form,
        CancellationToken cancellationToken)
    {
        var antiforgeryFailure = await TryRedirectOnAntiforgeryFailureAsync(form.ReturnUrl, cancellationToken);
        if (antiforgeryFailure is not null)
        {
            return antiforgeryFailure;
        }

        if (string.IsNullOrWhiteSpace(form.ProviderAlias) || string.IsNullOrWhiteSpace(form.IdToken))
        {
            return RedirectToLogin(form.ReturnUrl, "invalid_provider");
        }

        try
        {
            var login = await _externalLogin.LoginWithProviderAsync(form.ProviderAlias, form.IdToken, cancellationToken);
            return await CompleteLoginAsync(login, form.ReturnUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is Domain.Exceptions.DomainBusinessRuleException
            or Domain.Exceptions.DomainNotFoundException
            or Application.Exceptions.UnauthorizedApplicationException
            or Domain.Exceptions.DomainValidationException)
        {
            return RedirectToLogin(form.ReturnUrl, "invalid_provider");
        }
    }

    /// <summary>
    /// Signs out the browser session cookie.
    /// </summary>
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("/account/logout")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!await _antiforgery.IsRequestValidAsync(HttpContext))
        {
            return Redirect("/account/login");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private async Task<IActionResult?> TryRedirectOnAntiforgeryFailureAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        if (await _antiforgery.IsRequestValidAsync(HttpContext))
        {
            return null;
        }

        return RedirectToLogin(returnUrl, "session_expired");
    }

    private IActionResult RedirectToLogin(string? returnUrl, string errorCode)
    {
        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["returnUrl"] = returnUrl,
            ["error"] = errorCode
        }.Where(p => !string.IsNullOrWhiteSpace(p.Value)));
        return Redirect($"/account/login{query}");
    }

    private async Task<IActionResult> CompleteLoginAsync(
        ExternalLoginResult login,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var activeMembership = login.TenantMemberships.FirstOrDefault();
        var session = new AuthSession(
            login.UserId,
            clientId: null,
            activeMembership?.TenantId,
            activeMembership?.MembershipId,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString());

        await _sessions.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var context = new OidcLoginContext
        {
            Login = login,
            SessionId = session.Id,
            ActiveTenantId = activeMembership?.TenantId,
            ActiveMembershipId = activeMembership?.MembershipId
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, login.UserId.ToString("D")),
            new(ClaimTypes.Name, login.DisplayName),
            new(ClaimTypes.Email, login.Email),
            new("idp_login", JsonSerializer.Serialize(context))
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var redirectTarget = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = redirectTarget
            });

        return LocalRedirect(redirectTarget);
    }
}
