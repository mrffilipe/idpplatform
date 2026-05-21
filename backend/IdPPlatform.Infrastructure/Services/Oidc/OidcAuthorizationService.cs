using System.Security.Cryptography;
using System.Text.Json;
using IdPPlatform.Application.Services.Oidc;
using IdPPlatform.Application.Services.RefreshTokenHasher;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Repositories;

namespace IdPPlatform.Infrastructure.Services.Oidc;

public sealed class OidcAuthorizationService : IOidcAuthorizationService
{
    private static readonly TimeSpan AuthorizationCodeLifetime = TimeSpan.FromMinutes(10);

    private readonly IOidcAuthorizationCodeRepository _authorizationCodes;
    private readonly IRefreshTokenHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;

    public OidcAuthorizationService(
        IOidcAuthorizationCodeRepository authorizationCodes,
        IRefreshTokenHasher hasher,
        IUnitOfWork unitOfWork)
    {
        _authorizationCodes = authorizationCodes;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
    }

    public OidcError? ValidateAuthorizeRequest(
        OidcAuthorizeRequest request,
        ApplicationClientValidationContext clientContext)
    {
        if (!string.Equals(request.ResponseType, "code", StringComparison.Ordinal))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = "response_type must be code."
            };
        }

        return null;
    }

    public async Task<(string? Code, OidcError? Error)> CreateAuthorizationCodeAsync(
        OidcAuthorizeRequest request,
        Guid authSessionId,
        Guid applicationClientId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var code = GenerateSecret();
        var entity = new OidcAuthorizationCode(
            _hasher.Hash(code),
            applicationClientId,
            authSessionId,
            request.RedirectUri,
            JsonSerializer.Serialize(scopes),
            request.CodeChallenge ?? string.Empty,
            request.CodeChallengeMethod ?? OidcConstants.CodeChallengeMethodS256,
            request.Nonce,
            DateTime.UtcNow.Add(AuthorizationCodeLifetime));

        await _authorizationCodes.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (code, null);
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
