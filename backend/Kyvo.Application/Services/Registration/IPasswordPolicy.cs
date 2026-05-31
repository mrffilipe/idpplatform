namespace Kyvo.Application.Services.Registration;

/// <summary>
/// Contract for password policy enforcement. Implementations are configured through
/// <c>PasswordPolicyOptions</c> in Infrastructure and may evolve independently of the application code.
/// </summary>
public interface IPasswordPolicy
{
    PasswordPolicyResult Validate(string password);
}

public sealed record PasswordPolicyResult
{
    public required bool IsValid { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public static PasswordPolicyResult Success { get; } = new() { IsValid = true, Errors = Array.Empty<string>() };

    public static PasswordPolicyResult Failed(IReadOnlyList<string> errors) =>
        new() { IsValid = false, Errors = errors };
}
