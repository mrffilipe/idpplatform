using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Configurations;

public sealed class SecretProtectionOptionsValidator : IValidateOptions<SecretProtectionOptions>
{
    public ValidateOptionsResult Validate(string? name, SecretProtectionOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.KeyDirectoryPath))
        {
            errors.Add("SecretProtection:KeyDirectoryPath is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            errors.Add("SecretProtection:ApplicationName is required.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
