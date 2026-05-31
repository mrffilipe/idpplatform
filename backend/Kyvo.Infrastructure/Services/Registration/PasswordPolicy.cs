using Kyvo.Application.Services.Registration;
using Kyvo.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Services.Registration;

public sealed class PasswordPolicy : IPasswordPolicy
{
    private readonly PasswordPolicyOptions _options;

    public PasswordPolicy(IOptions<PasswordPolicyOptions> options)
    {
        _options = options.Value;
    }

    public PasswordPolicyResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return PasswordPolicyResult.Failed(errors);
        }

        if (password.Length < _options.MinLength)
        {
            errors.Add($"Password must be at least {_options.MinLength} characters long.");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (_options.RequireLetter && !password.Any(char.IsLetter))
        {
            errors.Add("Password must contain at least one letter.");
        }

        return errors.Count == 0 ? PasswordPolicyResult.Success : PasswordPolicyResult.Failed(errors);
    }
}
