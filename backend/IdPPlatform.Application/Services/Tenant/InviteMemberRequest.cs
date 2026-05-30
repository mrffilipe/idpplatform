namespace IdPPlatform.Application.Services.Tenant;

public sealed record InviteMemberRequest
{
    public Guid TenantId { get; init; }

    public required string Email { get; init; }

    public required IReadOnlyCollection<string> Roles { get; init; }

    public Guid InvitedByUserId { get; init; }

    public Guid ActorUserId { get; init; }

    public IReadOnlyCollection<string> ActorPlatformRoles { get; init; } = [];
}
