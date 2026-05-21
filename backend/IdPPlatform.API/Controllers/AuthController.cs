using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.Oidc;
using IdPPlatform.Application.Services.UserScope;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Route("v{version:apiVersion}/auth")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class AuthController : V1ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOidcTokenService _tokenService;
    private readonly IUserScope _userScope;

    public AuthController(
        IAuthService authService,
        IOidcTokenService tokenService,
        IUserScope userScope)
    {
        _authService = authService;
        _tokenService = tokenService;
        _userScope = userScope;
    }

    [Authorize]
    [HttpPost("subscribe")]
    public async Task<IActionResult> SubscribeTenant(
        [FromBody] SubscribeTenantBody body,
        CancellationToken cancellationToken)
    {
        var result = await _authService.SubscribeTenantAsync(
            new SubscribeTenantRequest
            {
                TenantName = body.TenantName,
                TenantKey = body.TenantKey,
                PlanCode = body.PlanCode,
                ExternalCustomerId = body.ExternalCustomerId
            },
            cancellationToken);

        object? tokens = null;
        if (_userScope.SessionId.HasValue)
        {
            var (tokenResponse, tokenError) = await _tokenService.IssueForSessionAsync(
                _userScope.SessionId.Value,
                cancellationToken);
            if (tokenError is null && tokenResponse is not null)
            {
                tokens = new
                {
                    access_token = tokenResponse.AccessToken,
                    refresh_token = tokenResponse.RefreshToken,
                    expires_in = tokenResponse.ExpiresIn,
                    token_type = tokenResponse.TokenType,
                    id_token = tokenResponse.IdToken,
                    scope = tokenResponse.Scope
                };
            }
        }

        return Ok(new
        {
            result.UserId,
            result.Email,
            result.TenantId,
            result.MembershipId,
            result.TenantRoles,
            result.PlatformRoles,
            result.Tenants,
            tokens
        });
    }

    [Authorize]
    [HttpPost("switch-tenant")]
    public async Task<IActionResult> SwitchTenant(
        [FromBody] SwitchTenantBody body,
        CancellationToken cancellationToken)
    {
        var result = await _authService.SwitchTenantAsync(
            new SwitchTenantRequest { TenantId = body.TenantId },
            cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> ListActiveSessions(CancellationToken cancellationToken)
    {
        var result = await _authService.ListActiveSessionsAsync(_userScope.UserId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await _authService.RevokeSessionAsync(_userScope.UserId, sessionId, cancellationToken);
        return NoContent();
    }

    public sealed record SubscribeTenantBody(
        string TenantName,
        string TenantKey,
        string? PlanCode,
        string? ExternalCustomerId);

    public sealed record SwitchTenantBody(Guid TenantId);
}
