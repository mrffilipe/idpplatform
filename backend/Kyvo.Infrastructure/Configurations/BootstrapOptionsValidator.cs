using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Configurations;

/// <summary>
/// Validates <see cref="BootstrapOptions"/>: bootstrap is optional, but when any field is
/// provided the email and password must both be present and the email must be syntactically valid.
/// </summary>
public sealed class BootstrapOptionsValidator : IValidateOptions<BootstrapOptions>
{
    public ValidateOptionsResult Validate(string? name, BootstrapOptions options)
    {
        var anyProvided =
            !string.IsNullOrWhiteSpace(options.AdminEmail) ||
            !string.IsNullOrWhiteSpace(options.AdminPassword) ||
            !string.IsNullOrWhiteSpace(options.AdminDisplayName);

        if (!anyProvided)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.AdminEmail))
        {
            errors.Add("Bootstrap:AdminEmail is required when any bootstrap option is provided.");
        }
        else if (!MailAddress.TryCreate(options.AdminEmail, out _))
        {
            errors.Add("Bootstrap:AdminEmail must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            errors.Add("Bootstrap:AdminPassword is required when any bootstrap option is provided.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
