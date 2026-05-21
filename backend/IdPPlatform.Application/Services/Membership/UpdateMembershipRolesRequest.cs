namespace IdPPlatform.Application.Services.Membership;

public sealed record UpdateMembershipRolesRequest
{
    public required Guid MembershipId { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }
}
