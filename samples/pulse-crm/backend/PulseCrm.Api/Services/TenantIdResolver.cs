using IdPPlatform.AspNetCore;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Data;

namespace PulseCrm.Api.Services;

internal static class TenantIdResolver
{
    public static async Task<Guid?> ResolveAsync(
        IIdPUserContext user,
        PulseCrmDbContext db,
        CancellationToken cancellationToken = default)
    {
        if (user.TenantId.HasValue)
        {
            return user.TenantId;
        }

        if (user.UserId is null)
        {
            return null;
        }

        var subscription = await db.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == user.UserId, cancellationToken);

        return subscription?.TenantId;
    }
}
