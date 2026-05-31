using Kyvo.Domain.Exceptions;
using Kyvo.Domain.Interfaces;

namespace Kyvo.Domain.Common;

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
