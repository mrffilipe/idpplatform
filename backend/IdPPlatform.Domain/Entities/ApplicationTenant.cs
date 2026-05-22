using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public sealed class ApplicationTenant : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; } = null!;

    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    /// <summary>
    /// Identifier of the tenant or customer in the consumer application's external system
    /// (e.g., CRM ID, Stripe customer id, account on the SaaS product).
    /// Optional; correlates the IdP tenant with billing or onboarding records of the app.
    /// Set on provisioning (platform admin) and subscribe (user authenticated via the app's OAuth).
    /// </summary>
    public string? ExternalCustomerId { get; private set; }

    /// <summary>
    /// Plan code or commercial contract associated with the application-tenant link
    /// (e.g., starter, enterprise). Optional; metadata for provisioning, feature limits
    /// or billing integration. Does not affect IdP authorization.
    /// </summary>
    public string? PlanCode { get; private set; }

    public bool IsActive { get; private set; }

    private ApplicationTenant()
    {
    }

    public ApplicationTenant(
        Guid applicationId,
        Guid tenantId,
        string? externalCustomerId,
        string? planCode)
    {
        if (applicationId == Guid.Empty || tenantId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.ApplicationTenant.DataInvalid);
        }

        ApplicationId = applicationId;
        TenantId = tenantId;
        ExternalCustomerId = string.IsNullOrWhiteSpace(externalCustomerId) ? null : externalCustomerId.Trim();
        PlanCode = string.IsNullOrWhiteSpace(planCode) ? null : planCode.Trim();
        IsActive = true;
    }
}
