namespace IdPPlatform.Application.Services.AppService;

public sealed record ProvisionApplicationTenantRequest
{
    public required Guid ApplicationId { get; init; }

    public required string TenantName { get; init; }

    public required string TenantKey { get; init; }

    public Guid? InitialAdministratorUserId { get; init; }

    public string? ExternalCustomerId { get; init; }

    public string? PlanCode { get; init; }

    public required Guid ActorUserId { get; init; }

    public required IReadOnlyList<string> ActorPlatformRoles { get; init; }
}
