namespace Kyvo.Application.Services.AppService;

public sealed record ProvisionApplicationTenantResult
{
    public required Guid ApplicationId { get; init; }

    public required Guid TenantId { get; init; }

    public required Guid MembershipId { get; init; }
}
