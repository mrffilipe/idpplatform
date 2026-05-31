using System.Text.Json;
using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.Oidc;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Repositories;

namespace Kyvo.Infrastructure.Services.Oidc;

public sealed class OidcClientValidator : IOidcClientValidator
{
    private readonly IApplicationClientRepository _clients;

    public OidcClientValidator(IApplicationClientRepository clients)
    {
        _clients = clients;
    }

    public async Task<(ApplicationClient? Client, OidcError? Error)> ValidateClientAsync(
        string? clientId,
        string? clientSecret,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return (null, new OidcError
            {
                Error = OidcConstants.Errors.InvalidClient,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.ClientIdRequired
            });
        }

        var client = await _clients.GetByClientIdAsync(clientId, cancellationToken);
        if (client is null)
        {
            return (null, new OidcError
            {
                Error = OidcConstants.Errors.InvalidClient,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.ClientIdInvalid
            });
        }

        if (client.ClientType == ClientType.Public)
        {
            if (!string.IsNullOrWhiteSpace(clientSecret))
            {
                return (null, new OidcError
                {
                    Error = OidcConstants.Errors.InvalidClient,
                    ErrorDescription = ApplicationErrorMessages.OAuthClient.ClientSecretRequired
                });
            }

            return (client, null);
        }

        if (string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(client.ClientSecretHash))
        {
            return (null, new OidcError
            {
                Error = OidcConstants.Errors.InvalidClient,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.ClientSecretRequired
            });
        }

        var valid = client.ClientSecretHash.StartsWith("$2", StringComparison.Ordinal)
            ? BCrypt.Net.BCrypt.Verify(clientSecret, client.ClientSecretHash)
            : string.Equals(clientSecret, client.ClientSecretHash, StringComparison.Ordinal);

        if (!valid)
        {
            return (null, new OidcError
            {
                Error = OidcConstants.Errors.InvalidClient,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.ClientSecretInvalid
            });
        }

        return (client, null);
    }

    public OidcError? ValidateRedirectUri(ApplicationClient client, string? redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.RedirectUriNotAllowed
            };
        }

        var allowed = DeserializeList(client.RedirectUris);
        if (!allowed.Any(uri => string.Equals(uri, redirectUri, StringComparison.Ordinal)))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.RedirectUriNotAllowed
            };
        }

        return null;
    }

    public OidcError? ValidateScopes(ApplicationClient client, IReadOnlyList<string> requestedScopes)
    {
        if (requestedScopes.Count == 0)
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidScope,
                ErrorDescription = ApplicationErrorMessages.OAuthClient.AllowedScopesRequired
            };
        }

        var allowed = DeserializeList(client.AllowedScopes);
        var disallowed = requestedScopes
            .Where(scope => !allowed.Contains(scope, StringComparer.Ordinal))
            .ToList();

        if (disallowed.Count > 0)
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidScope,
                ErrorDescription = string.Format(
                    ApplicationErrorMessages.OAuthClient.RequestedScopesNotAllowed,
                    string.Join(", ", disallowed))
            };
        }

        return null;
    }

    public OidcError? ValidatePkceForAuthorize(
        ApplicationClient client,
        string? codeChallenge,
        string? codeChallengeMethod)
    {
        var challengeError = PkceHelper.ValidateCodeChallenge(codeChallenge);
        if (challengeError is not null)
        {
            return challengeError;
        }

        return PkceHelper.ValidateCodeChallengeMethod(codeChallengeMethod);
    }

    public OidcError? ValidatePkceForToken(string codeChallenge, string codeChallengeMethod, string? codeVerifier)
    {
        var methodError = PkceHelper.ValidateCodeChallengeMethod(codeChallengeMethod);
        if (methodError is not null)
        {
            return methodError;
        }

        if (!PkceHelper.ValidateS256(codeChallenge, codeVerifier ?? string.Empty))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidGrant,
                ErrorDescription = ApplicationErrorMessages.Pkce.CodeChallengeRequired
            };
        }

        return null;
    }

    public IReadOnlyList<string> ParseScopes(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return [];
        }

        return scope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> DeserializeList(string rawJson)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(rawJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
