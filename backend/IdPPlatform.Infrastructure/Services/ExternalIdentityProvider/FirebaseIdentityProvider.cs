using FirebaseAdmin.Auth;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.ExternalIdentityProvider;

namespace IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;

public sealed class FirebaseIdentityProvider : IExternalIdentityProvider
{
    private readonly FirebaseAuth _firebaseAuth;

    public FirebaseIdentityProvider(FirebaseAuth firebaseAuth)
    {
        _firebaseAuth = firebaseAuth;
    }

    public async Task<ExternalAuthResult> ValidateAsync(
        string identityToken,
        CancellationToken cancellationToken = default)
    {
        FirebaseToken token;
        try
        {
            token = await _firebaseAuth.VerifyIdTokenAsync(identityToken, cancellationToken);
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
            Provider = "firebase",
            ProviderUserId = token.Uid,
            Email = email,
            EmailVerified = true,
            AuthenticationMethods = ["pwd"]
        };
    }
}
