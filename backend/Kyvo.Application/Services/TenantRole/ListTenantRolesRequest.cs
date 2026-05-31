using Kyvo.Application.Common;

namespace Kyvo.Application.Services.TenantRoles;

public sealed record ListTenantRolesRequest : PagedRequest
{
    public Guid TenantId { get; init; }

    public bool IncludeInactive { get; init; }
}
