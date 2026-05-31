using Kyvo.Domain.Common;

namespace Kyvo.Domain.Entities;

public class OidcRefreshToken : BaseEntity
{
    public string TokenHash { get; private set; } = string.Empty;

    public Guid ApplicationClientId { get; private set; }
    public ApplicationClient ApplicationClient { get; private set; } = null!;

    public Guid AuthSessionId { get; private set; }
    public AuthSession AuthSession { get; private set; } = null!;

    public string Scopes { get; private set; } = "[]";
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private OidcRefreshToken()
    {
    }

    public OidcRefreshToken(
        string tokenHash,
        Guid applicationClientId,
        Guid authSessionId,
        string scopes,
        DateTime expiresAt)
    {
        TokenHash = tokenHash;
        ApplicationClientId = applicationClientId;
        AuthSessionId = authSessionId;
        Scopes = scopes;
        ExpiresAt = expiresAt;
    }

    public bool IsValid(DateTime utcNow) => RevokedAt is null && utcNow < ExpiresAt;

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
