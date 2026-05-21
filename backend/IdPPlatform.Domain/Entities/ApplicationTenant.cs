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
    /// Identificador do tenant ou cliente no sistema externo da aplicação consumidora
    /// (ex.: ID no CRM, Stripe customer id, conta no produto SaaS).
    /// Opcional; correlaciona o tenant do IdP com registros de billing ou onboarding da app.
    /// Preenchido em provisionamento (admin de plataforma) e subscribe (usuário autenticado via OAuth da app).
    /// </summary>
    public string? ExternalCustomerId { get; private set; }

    /// <summary>
    /// Código do plano ou contrato comercial associado ao vínculo aplicação-tenant
    /// (ex.: starter, enterprise). Opcional; metadado para provisionamento, limites de features
    /// ou integração com billing. Não altera autorização no IdP.
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
