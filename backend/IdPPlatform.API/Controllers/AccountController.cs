using System.Security.Claims;
using System.Text.Json;
using IdPPlatform.API.Common;
using IdPPlatform.API.Models;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.LocalAuthentication;
using LocalLoginResult = IdPPlatform.Application.Services.LocalAuthentication.LocalLoginResult;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Infrastructure.Configurations;
using IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;
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
    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IAuthSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public AccountController(
        ILocalAuthenticationService localAuth,
        IExternalLoginService externalLogin,
        IIdentityProviderRepository identityProviders,
        IAuthSessionRepository sessions,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _localAuth = localAuth;
        _externalLogin = externalLogin;
        _identityProviders = identityProviders;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpGet("/account/login")]
    public async Task<IActionResult> Login([FromQuery] string? returnUrl, CancellationToken cancellationToken)
    {
        var model = await BuildLoginViewModelAsync(returnUrl, cancellationToken);
        return View(model);
    }

    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email e senha são obrigatórios.");
            return View(await BuildLoginViewModelAsync(returnUrl, cancellationToken));
        }

        var login = await _localAuth.LoginAsync(
            new LocalLoginRequest { Email = email, Password = password },
            cancellationToken);

        if (login is null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            return View(await BuildLoginViewModelAsync(returnUrl, cancellationToken));
        }

        return await CompleteLoginAsync(login.ToExternalLoginResult(), returnUrl, cancellationToken);
    }

    [HttpPost("/account/external-login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(
        [FromForm] string providerAlias,
        [FromForm] string idToken,
        [FromForm] string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerAlias) || string.IsNullOrWhiteSpace(idToken))
        {
            ModelState.AddModelError(string.Empty, "Provedor ou token inválido.");
            return View(await BuildLoginViewModelAsync(returnUrl, cancellationToken));
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
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await BuildLoginViewModelAsync(returnUrl, cancellationToken));
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

    private async Task<AccountLoginViewModel> BuildLoginViewModelAsync(
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var showLocal = await _identityProviders.AnyEnabledLocalProviderAsync(cancellationToken);
        var enabled = await _identityProviders.ListEnabledAsync(cancellationToken);

        var federated = enabled
            .Where(p => p.ProviderType != IdentityProviderType.Local)
            .Select(MapFederatedProvider)
            .ToList();

        return new AccountLoginViewModel
        {
            ReturnUrl = returnUrl,
            ShowLocalLogin = showLocal,
            FederatedProviders = federated
        };
    }

    private static FederatedProviderViewModel MapFederatedProvider(Domain.Entities.IdentityProvider provider)
    {
        IReadOnlyDictionary<string, string>? clientConfig = provider.ProviderType switch
        {
            IdentityProviderType.Firebase => BuildFirebaseClientConfig(provider.ConfigJson),
            _ => null
        };

        return new FederatedProviderViewModel
        {
            Alias = provider.Alias,
            DisplayName = provider.DisplayName,
            ProviderType = provider.ProviderType.ToString(),
            ClientConfig = clientConfig
        };
    }

    private static IReadOnlyDictionary<string, string>? BuildFirebaseClientConfig(string? configJson)
    {
        try
        {
            var config = FirebaseTokenValidator.DeserializeConfig(configJson);
            if (string.IsNullOrWhiteSpace(config.ProjectId) || string.IsNullOrWhiteSpace(config.WebApiKey))
            {
                return null;
            }

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["projectId"] = config.ProjectId,
                ["webApiKey"] = config.WebApiKey,
                ["authDomain"] = config.ResolveAuthDomain()
            };
        }
        catch
        {
            return null;
        }
    }
}

internal static class LocalLoginResultExtensions
{
    public static ExternalLoginResult ToExternalLoginResult(this LocalLoginResult login) =>
        new()
        {
            UserId = login.UserId,
            Email = login.Email,
            DisplayName = login.DisplayName,
            PlatformRoles = login.PlatformRoles,
            TenantMemberships = login.TenantMemberships
        };
}
