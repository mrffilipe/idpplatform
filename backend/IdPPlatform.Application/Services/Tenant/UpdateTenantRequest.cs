namespace IdPPlatform.Application.Services.Tenant;

public sealed record UpdateTenantRequest
{
    public Guid TenantId { get; init; }

    public required string Name { get; init; }

    public Guid ActorUserId { get; init; }

    public IReadOnlyCollection<string> ActorPlatformRoles { get; init; } = [];
}
