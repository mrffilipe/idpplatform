namespace IdPPlatform.Application.Services.Auth;

public sealed record SwitchTenantRequest
{
    public required Guid TenantId { get; init; }
}
