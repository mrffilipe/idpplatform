using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Users;

public sealed record ListUserMembershipsRequest : PagedRequest
{
    public Guid UserId { get; init; }
}
