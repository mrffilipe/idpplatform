using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Configurations;

public sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.FromAddress))
        {
            errors.Add("Email:FromAddress is required.");
        }
        else if (!MailAddress.TryCreate(options.FromAddress, out _))
        {
            errors.Add("Email:FromAddress must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            errors.Add("Email:Region is required.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
