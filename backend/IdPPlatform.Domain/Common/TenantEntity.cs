using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Interfaces;

namespace IdPPlatform.Domain.Common;

public abstract class TenantEntity : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    protected TenantEntity()
    {
    }

    protected TenantEntity(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantEntity.TenantIdRequired);
        }

        TenantId = tenantId;
    }
}
