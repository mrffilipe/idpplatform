using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Configurations;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            errors.Add("Jwt:Issuer is required.");
        }
        else if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out _))
        {
            errors.Add("Jwt:Issuer must be an absolute URI (e.g. http://localhost:5000).");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            errors.Add("Jwt:Audience is required.");
        }

        var hasPath = !string.IsNullOrWhiteSpace(options.SigningKeyPath);
        var hasPem = !string.IsNullOrWhiteSpace(options.SigningKeyPem);
        if (!hasPath && !hasPem)
        {
            errors.Add("Jwt:SigningKeyPath or Jwt:SigningKeyPem is required.");
        }
        else if (hasPath && hasPem)
        {
            errors.Add("Configure only one of Jwt:SigningKeyPath or Jwt:SigningKeyPem.");
        }

        if (string.IsNullOrWhiteSpace(options.KeyId))
        {
            errors.Add("Jwt:KeyId is required.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
