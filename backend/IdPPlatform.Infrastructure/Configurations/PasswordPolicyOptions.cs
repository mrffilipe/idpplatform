namespace IdPPlatform.Infrastructure.Configurations;

/// <summary>
/// Tunable password policy enforced by <c>PasswordPolicy</c> on self-registration and password updates.
/// </summary>
public sealed class PasswordPolicyOptions
{
    public const string Section = "PasswordPolicy";

    public int MinLength { get; init; } = 12;

    public bool RequireDigit { get; init; } = true;

    public bool RequireLetter { get; init; } = true;
}
