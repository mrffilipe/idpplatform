namespace PulseCrm.Api.Data;

public sealed class Subscription
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public Guid MembershipId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string TenantKey { get; set; } = string.Empty;

    public string PlanCode { get; set; } = string.Empty;

    public string? ExternalCustomerId { get; set; }

    public DateTime PaidAt { get; set; }
}
