using System.Security.Cryptography;
using System.Text;
using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.Oidc;

namespace Kyvo.Infrastructure.Services.Oidc;

internal static class PkceHelper
{
    public static bool ValidateS256(string codeChallenge, string codeVerifier)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier))
        {
            return false;
        }

        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        var computed = Base64UrlEncode(hash);
        return string.Equals(computed, codeChallenge, StringComparison.Ordinal);
    }

    public static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static OidcError? ValidateCodeChallenge(string? codeChallenge)
    {
        if (string.IsNullOrWhiteSpace(codeChallenge))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.Pkce.CodeChallengeRequired
            };
        }

        if (codeChallenge.Length is < 43 or > 128)
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.Pkce.CodeChallengeLength
            };
        }

        return null;
    }

    public static OidcError? ValidateCodeChallengeMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.Pkce.CodeChallengeMethodUnsupported
            };
        }

        if (!string.Equals(method, OidcConstants.CodeChallengeMethodS256, StringComparison.OrdinalIgnoreCase))
        {
            return new OidcError
            {
                Error = OidcConstants.Errors.InvalidRequest,
                ErrorDescription = ApplicationErrorMessages.Pkce.CodeChallengeMethodUnsupported
            };
        }

        return null;
    }
}
