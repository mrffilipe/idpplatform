namespace IdPPlatform.Infrastructure.Configurations;

public sealed class InviteOptions
{
    public const string Section = "Invite";

    public int ExpirationHours { get; init; } = 72;
}
