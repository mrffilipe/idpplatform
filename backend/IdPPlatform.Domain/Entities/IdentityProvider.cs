using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public sealed class IdentityProvider : BaseEntity
{
    public string Alias { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public IdentityProviderType ProviderType { get; private set; }
    public bool Enabled { get; private set; }
    public string? ConfigJson { get; private set; }

    private IdentityProvider()
    {
    }

    public IdentityProvider(
        string alias,
        string displayName,
        IdentityProviderType providerType,
        bool enabled = true,
        string? configJson = null)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new DomainValidationException(DomainErrorMessages.IdentityProvider.AliasRequired);
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(alias.Trim(), @"^[a-z0-9_-]+$"))
        {
            throw new DomainValidationException(DomainErrorMessages.IdentityProvider.AliasInvalidFormat);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainValidationException(DomainErrorMessages.IdentityProvider.DisplayNameRequired);
        }

        Alias = alias.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();
        ProviderType = providerType;
        Enabled = enabled;
        ConfigJson = configJson;
    }

    public void Enable() => Enabled = true;

    public void Disable() => Enabled = false;

    public void UpdateConfig(string? configJson) => ConfigJson = configJson;

    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainValidationException(DomainErrorMessages.IdentityProvider.DisplayNameRequired);
        }

        DisplayName = displayName.Trim();
    }
}
