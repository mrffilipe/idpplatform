namespace Kyvo.Application.Services.Tenant;

public sealed record GetTenantByIdRequest
{
    public required Guid TenantId { get; init; }

    public required Guid ActorUserId { get; init; }

    public required IReadOnlyCollection<string> ActorPlatformRoles { get; init; }
}
