using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Configurations;

public sealed class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Database:ConnectionString is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
