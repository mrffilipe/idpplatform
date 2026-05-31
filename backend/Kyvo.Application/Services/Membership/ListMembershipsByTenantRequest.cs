using Kyvo.Application.Common;

namespace Kyvo.Application.Services.Membership;

public sealed record ListMembershipsByTenantRequest : PagedRequest
{
    public Guid TenantId { get; init; }
}
