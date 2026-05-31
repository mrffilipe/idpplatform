using Kyvo.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    private readonly IKyvoUserContext _user;
    private readonly PulseCrmDbContext _db;

    public MeController(IKyvoUserContext user, PulseCrmDbContext db)
    {
        _user = user;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (_user.UserId is null)
        {
            return Unauthorized();
        }

        var subscription = await _db.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == _user.UserId, cancellationToken);

        var effectiveTenantId = _user.TenantId ?? subscription?.TenantId;
        var effectiveMembershipId = _user.MembershipId ?? subscription?.MembershipId;

        return Ok(new
        {
            userId = _user.UserId,
            email = _user.Email,
            tenantId = effectiveTenantId,
            membershipId = effectiveMembershipId,
            jwtTenantId = _user.TenantId,
            jwtMembershipId = _user.MembershipId,
            tenantRoles = _user.TenantRoles,
            platformRoles = _user.PlatformRoles,
            hasSubscription = subscription is not null,
            subscription = subscription is null
                ? null
                : new
                {
                    subscription.Id,
                    subscription.CompanyName,
                    subscription.TenantKey,
                    subscription.PlanCode,
                    subscription.TenantId,
                    subscription.MembershipId,
                    subscription.ExternalCustomerId,
                    subscription.PaidAt
                }
        });
    }
}
