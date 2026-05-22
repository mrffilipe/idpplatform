using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public class Application : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public ApplicationType Type { get; private set; }

    /// <summary>
    /// Indicates that this application is managed by the platform and cannot be edited or removed via API.
    /// Example: the admin console created automatically during bootstrap.
    /// </summary>
    public bool IsSystem { get; private set; }

    public ICollection<ApplicationClient> Clients { get; private set; } = new List<ApplicationClient>();
    public ICollection<ApplicationTenant> Tenants { get; private set; } = new List<ApplicationTenant>();

    private Application()
    {
    }

    public Application(
        string name,
        string slug,
        ApplicationType type,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainValidationException(DomainErrorMessages.Application.NameAndSlugRequired);
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Type = type;
        IsSystem = isSystem;
    }
}
