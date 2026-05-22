using IdPPlatform.Application.Services.Security;
using Microsoft.AspNetCore.DataProtection;

namespace IdPPlatform.Infrastructure.Services.Security;

/// <summary>
/// <see cref="ISecretProtector"/> implementation backed by ASP.NET Core Data Protection.
/// Produces ciphertext prefixed with <c>enc:v1:</c> so callers can distinguish encrypted payloads
/// from legacy plain-text values during lazy migration.
/// </summary>
public sealed class DataProtectionSecretProtector : ISecretProtector
{
    public const string ProtectorPurpose = "IdP.IdentityProvider.ConfigJson";
    public const string Prefix = "enc:v1:";

    private readonly IDataProtector _protector;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(ProtectorPurpose);
    }

    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        return Prefix + _protector.Protect(plaintext);
    }

    public string Unprotect(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return IsProtected(value)
            ? _protector.Unprotect(value[Prefix.Length..])
            : value;
    }

    public bool IsProtected(string value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(Prefix, StringComparison.Ordinal);
}
