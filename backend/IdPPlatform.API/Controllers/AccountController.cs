using System.Security.Claims;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.LocalAuthentication;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdPPlatform.API.Controllers;

[AllowAnonymous]
public sealed class AccountController : Controller
{
    private readonly ILocalAuthenticationService _localAuth;
    private readonly IExternalLoginService _externalLogin;
    private readonly IAuthSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public AccountController(
        ILocalAuthenticationService localAuth,
        IExternalLoginService externalLogin,
        IAuthSessionRepository sessions,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _localAuth = localAuth;
        _externalLogin = externalLogin;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("/account/signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return RedirectToLogin(returnUrl, "missing_fields");
        }

        var login = await _localAuth.LoginAsync(
            new LocalLoginRequest { Email = email, Password = password },
            cancellationToken);

        if (login is null)
        {
            return RedirectToLogin(returnUrl, "invalid_credentials");
        }

        return await CompleteLoginAsync(login.ToExternalLoginResult(), returnUrl, cancellationToken);
    }

    [HttpPost("/account/external-signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(
        [FromForm] string providerAlias,
        [FromForm] string idToken,
        [FromForm] string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerAlias) || string.IsNullOrWhiteSpace(idToken))
        {
            return RedirectToLogin(returnUrl, "invalid_provider");
        }

        try
        {
            var login = await _externalLogin.LoginWithProviderAsync(providerAlias, idToken, cancellationToken);
            return await CompleteLoginAsync(login, returnUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is Domain.Exceptions.DomainBusinessRuleException
            or Domain.Exceptions.DomainNotFoundException
            or Application.Exceptions.UnauthorizedApplicationException
            or Domain.Exceptions.DomainValidationException)
        {
            return RedirectToLogin(returnUrl, "invalid_provider");
        }
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
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

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
            });

        return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}
