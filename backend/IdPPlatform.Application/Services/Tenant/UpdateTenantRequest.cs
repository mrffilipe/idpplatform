namespace IdPPlatform.Application.Services.Tenant;

public sealed record UpdateTenantRequest
{
    public required Guid TenantId { get; init; }

    public required string Name { get; init; }

    public required Guid ActorUserId { get; init; }

    public required IReadOnlyCollection<string> ActorPlatformRoles { get; init; }
}
