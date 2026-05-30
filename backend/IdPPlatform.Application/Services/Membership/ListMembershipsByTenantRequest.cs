using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Membership;

public sealed record ListMembershipsByTenantRequest : PagedRequest
{
    public Guid TenantId { get; init; }
}
