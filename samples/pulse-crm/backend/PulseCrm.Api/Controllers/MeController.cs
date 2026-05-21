using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;
using PulseCrm.Api.Services;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public sealed class MeController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly PulseCrmDbContext _db;

    public MeController(IUserContext user, PulseCrmDbContext db)
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

        return Ok(new
        {
            userId = _user.UserId,
            email = _user.Email,
            tenantId = _user.TenantId,
            membershipId = _user.MembershipId,
            tenantRoles = _user.TenantRoles,
            platformRoles = _user.PlatformRoles,
            claims = _user.AllClaims,
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
