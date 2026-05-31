using Kyvo.Domain.Enums;

namespace Kyvo.Application.Services.IdentityProvider;

/// <summary>
/// Encrypts and decrypts the sensitive fields inside an identity provider <c>ConfigJson</c>
/// (e.g., Firebase <c>ServiceAccount</c>, <c>WebApiKey</c>) so secrets never live in plain text at rest.
/// The JSON structure is preserved; only the values of well-known sensitive paths change.
/// </summary>
public interface IIdentityProviderConfigCipher
{
    /// <summary>
    /// Encrypts the sensitive fields of <paramref name="configJson"/> for the given provider type.
    /// Returns the input unchanged when there are no sensitive paths for the provider type.
    /// </summary>
    string? Encrypt(IdentityProviderType providerType, string? configJson);

    /// <summary>
    /// Decrypts any encrypted leaf values found at the top level of <paramref name="configJson"/>.
    /// Legacy plain-text payloads pass through untouched, enabling lazy migration to encryption at rest.
    /// </summary>
    string? Decrypt(string? configJson);
}
