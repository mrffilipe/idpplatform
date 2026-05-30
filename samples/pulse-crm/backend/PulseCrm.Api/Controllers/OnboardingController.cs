using IdPPlatform.AspNetCore;
using IdPPlatform.Client;
using IdPPlatform.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;
using PulseCrm.Api.Helpers;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/onboarding")]
public sealed class OnboardingController : ControllerBase
{
    private readonly IIdPUserContext _user;
    private readonly PulseCrmDbContext _db;
    private readonly IIdPProductClient _idp;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OnboardingController(
        IIdPUserContext user,
        PulseCrmDbContext db,
        IIdPProductClient idp,
        IHttpContextAccessor httpContextAccessor)
    {
        _user = user;
        _db = db;
        _idp = idp;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
        [FromBody] CompleteOnboardingBody body,
        CancellationToken cancellationToken)
    {
        if (_user.UserId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(body.CompanyName) || string.IsNullOrWhiteSpace(body.PlanCode))
        {
            return BadRequest(new { message = "companyName and planCode are required." });
        }

        var existing = await _db.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == _user.UserId, cancellationToken);

        if (existing is not null)
        {
            return Conflict(new { message = "User already completed onboarding.", subscription = existing });
        }

        var accessToken = _httpContextAccessor.GetUserAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized(new { message = "Bearer access token required." });
        }

        var tenantKey = SlugHelper.ToTenantKey(body.CompanyName);
        var externalCustomerId = body.PaymentReference ?? $"pay_mock_{Guid.NewGuid():N}"[..24];

        SubscribeTenantResult idpSubscribe;
        try
        {
            idpSubscribe = await _idp.Auth.SubscribeAsync(
                accessToken,
                new SubscribeTenantRequest(
                    body.CompanyName.Trim(),
                    tenantKey,
                    body.PlanCode.Trim().ToLowerInvariant(),
                    externalCustomerId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }

        var idpResult = idpSubscribe.Context;
        if (idpResult.TenantId is null || idpResult.MembershipId is null)
        {
            return StatusCode(502, new { message = "IdP subscribe did not return tenant context. Refresh token and retry." });
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = _user.UserId.Value,
            TenantId = idpResult.TenantId.Value,
            MembershipId = idpResult.MembershipId.Value,
            CompanyName = body.CompanyName.Trim(),
            TenantKey = tenantKey,
            PlanCode = body.PlanCode.Trim().ToLowerInvariant(),
            ExternalCustomerId = externalCustomerId,
            PaidAt = DateTime.UtcNow
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            subscription,
            idpTenantContext = idpResult,
            tokens = idpSubscribe.Tokens is null
                ? null
                : new
                {
                    access_token = idpSubscribe.Tokens.AccessToken,
                    refresh_token = idpSubscribe.Tokens.RefreshToken,
                    expires_in = idpSubscribe.Tokens.ExpiresIn,
                    token_type = idpSubscribe.Tokens.TokenType
                },
            requiresTokenRefresh = idpSubscribe.Tokens is null,
            message = idpSubscribe.Tokens is null
                ? "Onboarding complete. Refresh OIDC tokens to receive tid/mid claims."
                : "Onboarding complete. Session tokens include tid/mid."
        });
    }

    public sealed record CompleteOnboardingBody(
        string CompanyName,
        string PlanCode,
        string? PaymentReference);
}
