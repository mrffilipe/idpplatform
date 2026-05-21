using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Configurations;

public sealed class FirebaseOptionsValidator : IValidateOptions<FirebaseOptions>
{
    public ValidateOptionsResult Validate(string? name, FirebaseOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ProjectId))
        {
            return ValidateOptionsResult.Success;
        }

        return ValidateOptionsResult.Fail(
            "Configure Firebase:ProjectId in appsettings.");
    }
}
