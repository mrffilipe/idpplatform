using System.Security.Cryptography;
using System.Text.Json;
using Kyvo.Application.Services.Oidc;
using Kyvo.Application.Services.RefreshTokenHasher;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Repositories;
using Kyvo.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Services.Oidc;

public sealed class OidcTokenService : IOidcTokenService
{
    private readonly IOidcClientValidator _clientValidator;
    private readonly IOidcClaimsService _claimsService;
    private readonly IOidcAuthorizationCodeRepository _authorizationCodes;
    private readonly IOidcRefreshTokenRepository _refreshTokens;
    private readonly IAuthSessionRepository _sessions;
    private readonly IApplicationClientRepository _clients;
    private readonly IRefreshTokenHasher _hasher;
    private readonly IJwtSigningService _jwtSigning;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;

    public OidcTokenService(
        IOidcClientValidator clientValidator,
        IOidcClaimsService claimsService,
        IOidcAuthorizationCodeRepository authorizationCodes,
        IOidcRefreshTokenRepository refreshTokens,
        IAuthSessionRepository sessions,
        IApplicationClientRepository clients,
        IRefreshTokenHasher hasher,
        IJwtSigningService jwtSigning,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions)
    {
        _clientValidator = clientValidator;
        _claimsService = claimsService;
        _authorizationCodes = authorizationCodes;
        _refreshTokens = refreshTokens;
        _sessions = sessions;
        _clients = clients;
        _hasher = hasher;
        _jwtSigning = jwtSigning;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(OidcTokenResponse? Response, OidcError? Error)> IssueForSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetForUpdateAsync(sessionId, cancellationToken);
        if (session is null || session.Status != SessionStatus.Active)
        {
            return (null, InvalidGrant("Session is not active."));
        }

        if (!session.ClientId.HasValue)
        {
            return (null, InvalidGrant("Session has no OAuth client. Sign in again from the application."));
        }

        var client = await _clients.GetByIdAsync(session.ClientId.Value, cancellationToken);
        if (client is null)
        {
            return (null, InvalidGrant("OAuth client not found."));
        }

        var scopes = DeserializeScopes(client.AllowedScopes);
        if (scopes.Count == 0)
        {
            scopes =
            [
                OidcConstants.Scopes.OpenId,
                OidcConstants.Scopes.Profile,
                OidcConstants.Scopes.Email,
                OidcConstants.Scopes.OfflineAccess
            ];
        }

        session.Touch();
        return await IssueTokensAsync(client, sessionId, scopes, nonce: null, cancellationToken);
    }

    public async Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeAsync(
        OidcTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        return request.GrantType switch
        {
            "authorization_code" => await ExchangeAuthorizationCodeAsync(request, cancellationToken),
            "refresh_token" => await ExchangeRefreshTokenAsync(request, cancellationToken),
            _ => (null, new OidcError
            {
                Error = OidcConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            })
        };
    }

    private async Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeAuthorizationCodeAsync(
        OidcTokenRequest request,
        CancellationToken cancellationToken)
    {
        var (client, clientError) = await _clientValidator.ValidateClientAsync(
            request.ClientId,
            request.ClientSecret,
            cancellationToken);
        if (clientError is not null)
        {
            return (null, clientError);
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return (null, InvalidGrant("Authorization code is required."));
        }

        var redirectError = _clientValidator.ValidateRedirectUri(client!, request.RedirectUri);
        if (redirectError is not null)
        {
            return (null, redirectError);
        }

        var stored = await _authorizationCodes.GetByCodeHashForUpdateAsync(_hasher.Hash(request.Code), cancellationToken);
        if (stored is null || !stored.IsValid(DateTime.UtcNow))
        {
            return (null, InvalidGrant("Authorization code is invalid or expired."));
        }

        if (!string.Equals(stored.RedirectUri, request.RedirectUri, StringComparison.Ordinal))
        {
            return (null, InvalidGrant("redirect_uri does not match."));
        }

        if (stored.ApplicationClientId != client!.Id)
        {
            return (null, InvalidGrant("client_id does not match."));
        }

        var pkceError = _clientValidator.ValidatePkceForToken(
            stored.CodeChallenge,
            stored.CodeChallengeMethod,
            request.CodeVerifier);
        if (pkceError is not null)
        {
            return (null, pkceError);
        }

        stored.Consume();
        var scopes = DeserializeScopes(stored.Scopes);
        return await IssueTokensAsync(client, stored.AuthSessionId, scopes, stored.Nonce, cancellationToken);
    }

    private async Task<(OidcTokenResponse? Response, OidcError? Error)> ExchangeRefreshTokenAsync(
        OidcTokenRequest request,
        CancellationToken cancellationToken)
    {
        var (client, clientError) = await _clientValidator.ValidateClientAsync(
            request.ClientId,
            request.ClientSecret,
            cancellationToken);
        if (clientError is not null)
        {
            return (null, clientError);
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return (null, InvalidGrant("refresh_token is required."));
        }

        var stored = await _refreshTokens.GetByTokenHashForUpdateAsync(
            _hasher.Hash(request.RefreshToken),
            cancellationToken);
        if (stored is null || !stored.IsValid(DateTime.UtcNow))
        {
            return (null, InvalidGrant("Refresh token is invalid or expired."));
        }

        if (stored.ApplicationClientId != client!.Id)
        {
            return (null, InvalidGrant("client_id does not match."));
        }

        var session = await _sessions.GetForUpdateAsync(stored.AuthSessionId, cancellationToken);
        if (session is null || session.Status != SessionStatus.Active)
        {
            return (null, InvalidGrant("Session is no longer active."));
        }

        session.Touch();
        stored.Revoke();

        var scopes = DeserializeScopes(stored.Scopes);
        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            var requested = _clientValidator.ParseScopes(request.Scope);
            var scopeError = _clientValidator.ValidateScopes(client, requested);
            if (scopeError is not null)
            {
                return (null, scopeError);
            }

            scopes = requested;
        }

        return await IssueTokensAsync(client, stored.AuthSessionId, scopes, nonce: null, cancellationToken);
    }

    private async Task<(OidcTokenResponse? Response, OidcError? Error)> IssueTokensAsync(
        ApplicationClient client,
        Guid sessionId,
        IReadOnlyList<string> scopes,
        string? nonce,
        CancellationToken cancellationToken)
    {
        var claims = await _claimsService.TryBuildClaimsAsync(sessionId, scopes, cancellationToken);
        if (claims is null)
        {
            return (null, InvalidGrant("Unable to build token claims."));
        }

        var accessLifetime = TimeSpan.FromSeconds(client.AccessTokenTtlSeconds);
        var accessClaims = OidcClaimsBuilder.ForAccessToken(claims);
        var accessToken = _jwtSigning.SignAccessToken(accessClaims, accessLifetime);

        string? idToken = null;
        if (scopes.Contains(OidcConstants.Scopes.OpenId, StringComparer.Ordinal))
        {
            var idClaims = OidcClaimsBuilder.ForIdToken(claims);
            idToken = _jwtSigning.SignIdToken(idClaims, accessLifetime, client.ClientId, nonce);
        }

        string? refreshTokenValue = null;
        if (scopes.Contains(OidcConstants.Scopes.OfflineAccess, StringComparer.Ordinal))
        {
            refreshTokenValue = await PersistRefreshTokenAsync(client.Id, sessionId, scopes, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (new OidcTokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = (int)accessLifetime.TotalSeconds,
            RefreshToken = refreshTokenValue,
            IdToken = idToken,
            Scope = string.Join(' ', scopes)
        }, null);
    }

    private async Task<string> PersistRefreshTokenAsync(
        Guid applicationClientId,
        Guid sessionId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var entity = new OidcRefreshToken(
            _hasher.Hash(refreshToken),
            applicationClientId,
            sessionId,
            JsonSerializer.Serialize(scopes),
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays));

        await _refreshTokens.AddAsync(entity, cancellationToken);
        return refreshToken;
    }

    private static IReadOnlyList<string> DeserializeScopes(string scopesJson)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(scopesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static OidcError InvalidGrant(string description) => new()
    {
        Error = OidcConstants.Errors.InvalidGrant,
        ErrorDescription = description
    };
}
