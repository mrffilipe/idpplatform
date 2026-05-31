using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Configurations;

public sealed class InviteOptionsValidator : IValidateOptions<InviteOptions>
{
    public ValidateOptionsResult Validate(string? name, InviteOptions options)
    {
        if (options.ExpirationHours <= 0)
        {
            return ValidateOptionsResult.Fail("Invite:ExpirationHours must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
