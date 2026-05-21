namespace IdPPlatform.Application.Services.Auth;

public sealed record SubscribeTenantRequest
{
    public required string TenantName { get; init; }

    public required string TenantKey { get; init; }

    public string? PlanCode { get; init; }

    public string? ExternalCustomerId { get; init; }
}
