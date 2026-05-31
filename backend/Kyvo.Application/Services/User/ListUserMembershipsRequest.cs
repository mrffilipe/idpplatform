using Kyvo.Application.Common;

namespace Kyvo.Application.Services.Users;

public sealed record ListUserMembershipsRequest : PagedRequest
{
    public Guid UserId { get; init; }
}
