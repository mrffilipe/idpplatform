using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.TenantRoles;

public sealed record ListTenantRolesRequest : PagedRequest
{
    public required Guid TenantId { get; init; }

    public bool IncludeInactive { get; init; }
}
