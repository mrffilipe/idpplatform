using IdPPlatform.Domain.Common;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;

namespace IdPPlatform.Domain.Entities;

public class ApplicationClient : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Application Application { get; private set; } = null!;

    public string ClientId { get; private set; } = string.Empty;
    public string? ClientSecretHash { get; private set; }
    public ClientType ClientType { get; private set; }
    public string RedirectUris { get; private set; } = "[]";
    public string AllowedScopes { get; private set; } = "[]";
    public int AccessTokenTtlSeconds { get; private set; }

    /// <summary>
    /// Indicates that this client is managed by the platform and cannot be edited or removed via API.
    /// Example: the admin console client created automatically during bootstrap.
    /// </summary>
    public bool IsSystem { get; private set; }

    private ApplicationClient()
    {
    }

    public ApplicationClient(
        Guid applicationId,
        string clientId,
        string? clientSecretHash,
        ClientType clientType,
        string redirectUris,
        string allowedScopes,
        int accessTokenTtlSeconds,
        bool isSystem = false)
    {
        if (applicationId == Guid.Empty || string.IsNullOrWhiteSpace(clientId))
        {
            throw new DomainValidationException(DomainErrorMessages.ApplicationClient.DataInvalid);
        }

        ApplicationId = applicationId;
        ClientId = clientId.Trim();
        ClientSecretHash = clientSecretHash;
        ClientType = clientType;
        RedirectUris = string.IsNullOrWhiteSpace(redirectUris) ? "[]" : redirectUris;
        AllowedScopes = string.IsNullOrWhiteSpace(allowedScopes) ? "[]" : allowedScopes;
        AccessTokenTtlSeconds = accessTokenTtlSeconds > 0 ? accessTokenTtlSeconds : 900;
        IsSystem = isSystem;
    }
}
