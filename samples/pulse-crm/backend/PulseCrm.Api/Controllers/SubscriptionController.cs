using Kyvo.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/subscription")]
public sealed class SubscriptionController : ControllerBase
{
    private readonly IKyvoUserContext _user;
    private readonly PulseCrmDbContext _db;

    public SubscriptionController(IKyvoUserContext user, PulseCrmDbContext db)
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

        if (subscription is null)
        {
            return NotFound(new { message = "No subscription. Complete onboarding first." });
        }

        return Ok(subscription);
    }
}
