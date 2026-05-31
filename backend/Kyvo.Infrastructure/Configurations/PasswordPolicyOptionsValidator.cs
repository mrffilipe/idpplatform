using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Configurations;

public sealed class PasswordPolicyOptionsValidator : IValidateOptions<PasswordPolicyOptions>
{
    public ValidateOptionsResult Validate(string? name, PasswordPolicyOptions options)
    {
        if (options.MinLength < 8)
        {
            return ValidateOptionsResult.Fail("PasswordPolicy:MinLength must be at least 8.");
        }

        return ValidateOptionsResult.Success;
    }
}
