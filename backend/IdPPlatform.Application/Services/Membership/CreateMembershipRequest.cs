namespace IdPPlatform.Application.Services.Membership;

public sealed record CreateMembershipRequest
{
    public required Guid UserId { get; init; }

    public required Guid TenantId { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }
}
