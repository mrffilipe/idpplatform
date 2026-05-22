using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Configurations;

public sealed class RateLimitOptionsValidator : IValidateOptions<RateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitOptions options)
    {
        var errors = new List<string>();

        if (options.BootstrapPermitLimit <= 0)
        {
            errors.Add("RateLimit:BootstrapPermitLimit must be greater than zero.");
        }

        if (options.BootstrapWindowMinutes <= 0)
        {
            errors.Add("RateLimit:BootstrapWindowMinutes must be greater than zero.");
        }

        if (options.AccountRegisterPermitLimit <= 0)
        {
            errors.Add("RateLimit:AccountRegisterPermitLimit must be greater than zero.");
        }

        if (options.AccountRegisterWindowMinutes <= 0)
        {
            errors.Add("RateLimit:AccountRegisterWindowMinutes must be greater than zero.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
