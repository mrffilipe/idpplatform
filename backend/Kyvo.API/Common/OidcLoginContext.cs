using Kyvo.Application.Services.Auth;

namespace Kyvo.API.Common;

public sealed class OidcLoginContext
{
    public required ExternalLoginResult Login { get; init; }
    public required Guid SessionId { get; init; }
    public Guid? ActiveTenantId { get; init; }
    public Guid? ActiveMembershipId { get; init; }
}
