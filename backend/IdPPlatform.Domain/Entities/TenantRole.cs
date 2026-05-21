using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.ValueObjects;

namespace IdPPlatform.Domain.Entities;

public sealed class TenantRole : TenantEntity
{
    public TenantRoleKey Key { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private TenantRole()
    {
    }

    public TenantRole(
        Guid tenantId,
        TenantRoleKey key,
        string name,
        string? description = null,
        bool isSystem = false) : base(tenantId)
    {
        Key = key;
        SetDetails(name, description);
        IsSystem = isSystem;
        IsActive = true;
    }

    public void UpdateDetails(string name, string? description) => SetDetails(name, description);

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private void SetDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.NameRequired);
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > 120)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.NameMaxLength);
        }

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > 500)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.DescriptionMaxLength);
        }

        Name = normalizedName;
        Description = normalizedDescription;
    }
}
