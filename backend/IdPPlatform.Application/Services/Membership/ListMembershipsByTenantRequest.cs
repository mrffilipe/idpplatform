using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Membership;

public sealed record ListMembershipsByTenantRequest : PagedRequest
{
    public required Guid TenantId { get; init; }
}
