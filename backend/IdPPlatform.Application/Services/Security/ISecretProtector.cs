namespace IdPPlatform.Application.Services.Security;

/// <summary>
/// Protects (encrypts) and unprotects (decrypts) secret string values at rest.
/// Implementations must be reversible and produce a marker prefix that allows callers
/// to distinguish protected payloads from legacy plain-text values.
/// </summary>
public interface ISecretProtector
{
    string Protect(string plaintext);

    string Unprotect(string value);

    bool IsProtected(string value);
}
