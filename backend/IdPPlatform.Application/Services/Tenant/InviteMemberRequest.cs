namespace IdPPlatform.Application.Services.Tenant;

public sealed record InviteMemberRequest
{
    public required Guid TenantId { get; init; }

    public required string Email { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }

    public required Guid InvitedByUserId { get; init; }

    public required Guid ActorUserId { get; init; }

    public required IReadOnlyCollection<string> ActorPlatformRoles { get; init; }
}
