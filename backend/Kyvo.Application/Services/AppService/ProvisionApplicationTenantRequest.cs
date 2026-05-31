namespace Kyvo.Application.Services.AppService;

public sealed record ProvisionApplicationTenantRequest
{
    public Guid ApplicationId { get; init; }

    public required string TenantName { get; init; }

    public required string TenantKey { get; init; }

    public Guid? InitialAdministratorUserId { get; init; }

    public string? ExternalCustomerId { get; init; }

    public string? PlanCode { get; init; }

    public Guid ActorUserId { get; init; }

    public IReadOnlyList<string> ActorPlatformRoles { get; init; } = [];
}
