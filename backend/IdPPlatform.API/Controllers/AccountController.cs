using System.Security.Claims;
using System.Text.Json;
using IdPPlatform.API.Common;
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
    private readonly IApplicationClientRepository _clients;
    private readonly IAuthSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public AccountController(
        ILocalAuthenticationService localAuth,
        IApplicationClientRepository clients,
        IAuthSessionRepository sessions,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _localAuth = localAuth;
        _clients = clients;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpGet("/account/login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
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
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var login = await _localAuth.LoginAsync(
            new LocalLoginRequest { Email = email, Password = password },
            cancellationToken);

        if (login is null)
        {
            ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

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
            Login = new Application.Services.Auth.ExternalLoginResult
            {
                UserId = login.UserId,
                Email = login.Email,
                DisplayName = login.DisplayName,
                PlatformRoles = login.PlatformRoles,
                TenantMemberships = login.TenantMemberships
            },
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

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
