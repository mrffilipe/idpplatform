namespace IdPPlatform.Application.Services.Membership;

public sealed record UpdateMembershipRolesRequest
{
    public Guid MembershipId { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }
}
