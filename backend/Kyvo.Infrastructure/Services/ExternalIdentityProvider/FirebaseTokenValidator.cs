using System.Collections.Concurrent;
using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Kyvo.Application.Exceptions;
using Kyvo.Application.IdentityProviderConfigs;
using Kyvo.Application.Services.ExternalIdentityProvider;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Infrastructure.Services.ExternalIdentityProvider;

public sealed class FirebaseTokenValidator : IIdentityProviderTokenValidator
{
    private static readonly ConcurrentDictionary<string, FirebaseApp> AppCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IIdentityProviderConfigCipher _configCipher;

    public FirebaseTokenValidator(IIdentityProviderConfigCipher configCipher)
    {
        _configCipher = configCipher;
    }

    public async Task<ExternalAuthResult> ValidateAsync(
        Domain.Entities.IdentityProvider provider,
        string identityToken,
        CancellationToken cancellationToken = default)
    {
        var config = DeserializeConfig(_configCipher.Decrypt(provider.ConfigJson));
        var auth = GetOrCreateAuth(provider.Alias, config);

        FirebaseToken token;
        try
        {
            token = await auth.VerifyIdTokenAsync(identityToken, cancellationToken);
        }
        catch (Exception)
        {
            throw new UnauthorizedApplicationException(ApplicationErrorMessages.ExternalIdentity.InvalidToken);
        }

        var email = token.Claims.TryGetValue("email", out var emailObj) ? emailObj?.ToString() : null;
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedApplicationException(ApplicationErrorMessages.ExternalIdentity.EmailMissing);
        }

        return new ExternalAuthResult
        {
            Provider = provider.Alias,
            ProviderUserId = token.Uid,
            Email = email,
            EmailVerified = true,
            AuthenticationMethods = ["pwd"]
        };
    }

    public static FirebaseProviderConfig DeserializeConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }

        try
        {
            return JsonSerializer.Deserialize<FirebaseProviderConfig>(configJson, JsonOptions)
                ?? throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
        catch (JsonException)
        {
            throw new DomainValidationException(ApplicationErrorMessages.IdentityProvider.ConfigInvalid);
        }
    }

    private static FirebaseAuth GetOrCreateAuth(string alias, FirebaseProviderConfig config)
    {
        var app = AppCache.GetOrAdd(alias, _ =>
        {
            var serviceAccountJson = config.ServiceAccount.GetRawText();
            var credential = GoogleCredential.FromJson(serviceAccountJson);

            return FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = config.ProjectId
            }, alias);
        });

        return FirebaseAuth.GetAuth(app);
    }
}
