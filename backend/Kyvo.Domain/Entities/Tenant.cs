using Kyvo.Domain.Common;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.ValueObjects;

namespace Kyvo.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public TenantKey Key { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public ICollection<TenantMembership> Memberships { get; private set; } = new List<TenantMembership>();
    public ICollection<TenantRole> Roles { get; private set; } = new List<TenantRole>();
    public ICollection<ApplicationTenant> Applications { get; private set; } = new List<ApplicationTenant>();

    private Tenant()
    {
    }

    public Tenant(string name, TenantKey key)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException(DomainErrorMessages.Tenant.NameRequired);
        }

        Name = name.Trim();
        Key = key;
        IsActive = true;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException(DomainErrorMessages.Tenant.NameRequired);
        }

        Name = name.Trim();
    }
}
