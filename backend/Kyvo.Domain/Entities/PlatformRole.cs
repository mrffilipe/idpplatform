using Kyvo.Domain.Common;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Domain.Entities;

public sealed class PlatformRole : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsSystem { get; private set; }

    public ICollection<UserPlatformRole> UserAssignments { get; private set; } = new List<UserPlatformRole>();

    private PlatformRole()
    {
    }

    public PlatformRole(string key, string name, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformRole.KeyRequired);
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(key.Trim(), @"^[a-z0-9_]+$"))
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformRole.KeyInvalidFormat);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformRole.NameRequired);
        }

        if (name.Trim().Length > 120)
        {
            throw new DomainValidationException(DomainErrorMessages.PlatformRole.NameMaxLength);
        }

        Key = key.Trim().ToLowerInvariant();
        Name = name.Trim();
        IsSystem = isSystem;
    }
}
