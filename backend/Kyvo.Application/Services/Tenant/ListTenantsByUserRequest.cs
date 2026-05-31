using Kyvo.Application.Common;

namespace Kyvo.Application.Services.Tenant;

public sealed record ListTenantsByUserRequest : PagedRequest
{
    public Guid UserId { get; init; }
}
