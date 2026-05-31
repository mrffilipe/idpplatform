using Kyvo.API.Common;
using Kyvo.API.Models;
using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.Oidc;
using Kyvo.Application.Services.UserScope;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Controllers;

/// <summary>
/// Authenticated tenant context: subscribe, switch tenant, and session management.
/// </summary>
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

    /// <summary>
    /// Creates a tenant for the current OAuth application session (SaaS onboarding).
    /// </summary>
    /// <remarks>
    /// When the caller has an active auth session, freshly issued OAuth tokens may be included under <c>tokens</c>
    /// using RFC 6749 field names (<c>access_token</c>, <c>refresh_token</c>, etc.).
    /// </remarks>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(SubscribeTenantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubscribeTenantResponse>> SubscribeTenant(
        [FromBody] SubscribeTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.SubscribeTenantAsync(request, cancellationToken);

        OidcTokenResponse? tokens = null;
        if (_userScope.SessionId.HasValue)
        {
            var (tokenResponse, tokenError) = await _tokenService.IssueForSessionAsync(_userScope.SessionId.Value, cancellationToken);
            if (tokenError is null && tokenResponse is not null)
            {
                tokens = tokenResponse;
            }
        }

        return Ok(new SubscribeTenantResponse
        {
            UserId = result.UserId,
            Email = result.Email,
            TenantId = result.TenantId,
            MembershipId = result.MembershipId,
            TenantRoles = result.TenantRoles,
            PlatformRoles = result.PlatformRoles,
            Tenants = result.Tenants,
            Tokens = tokens
        });
    }

    /// <summary>
    /// Switches the active tenant for the current user session.
    /// </summary>
    [HttpPost("switch-tenant")]
    [ProducesResponseType(typeof(TenantContextResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantContextResult>> SwitchTenant(
        [FromBody] SwitchTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.SwitchTenantAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists active authentication sessions for the current user.
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IReadOnlyList<AuthSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuthSessionDto>>> ListActiveSessions(CancellationToken cancellationToken)
    {
        var result = await _authService.ListActiveSessionsAsync(_userScope.UserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Revokes a single authentication session.
    /// </summary>
    [HttpDelete("sessions/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await _authService.RevokeSessionAsync(_userScope.UserId, sessionId, cancellationToken);
        return NoContent();
    }
}
