using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Tenant;

public sealed record ListTenantsByUserRequest : PagedRequest
{
    public required Guid UserId { get; init; }
}
