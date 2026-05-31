using Kyvo.Domain.Common;

namespace Kyvo.Domain.Entities;

public class OidcAuthorizationCode : BaseEntity
{
    public string CodeHash { get; private set; } = string.Empty;

    public Guid ApplicationClientId { get; private set; }
    public ApplicationClient ApplicationClient { get; private set; } = null!;

    public Guid AuthSessionId { get; private set; }
    public AuthSession AuthSession { get; private set; } = null!;

    public string RedirectUri { get; private set; } = string.Empty;
    public string Scopes { get; private set; } = "[]";
    public string CodeChallenge { get; private set; } = string.Empty;
    public string CodeChallengeMethod { get; private set; } = "S256";
    public string? Nonce { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }

    private OidcAuthorizationCode()
    {
    }

    public OidcAuthorizationCode(
        string codeHash,
        Guid applicationClientId,
        Guid authSessionId,
        string redirectUri,
        string scopes,
        string codeChallenge,
        string codeChallengeMethod,
        string? nonce,
        DateTime expiresAt)
    {
        CodeHash = codeHash;
        ApplicationClientId = applicationClientId;
        AuthSessionId = authSessionId;
        RedirectUri = redirectUri;
        Scopes = scopes;
        CodeChallenge = codeChallenge;
        CodeChallengeMethod = codeChallengeMethod;
        Nonce = nonce;
        ExpiresAt = expiresAt;
    }

    public bool IsValid(DateTime utcNow) => ConsumedAt is null && utcNow < ExpiresAt;

    public void Consume() => ConsumedAt = DateTime.UtcNow;
}
