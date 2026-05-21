namespace IdPPlatform.Application.Services.Membership;

public sealed record RevokeMembershipRequest
{
    public required Guid MembershipId { get; init; }
}
