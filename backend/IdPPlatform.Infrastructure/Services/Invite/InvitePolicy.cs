using IdPPlatform.Application.Interfaces;
using IdPPlatform.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Services.Invite;

public sealed class InvitePolicy : IInvitePolicy
{
    public InvitePolicy(IOptions<InviteOptions> options)
    {
        ExpirationHours = options.Value.ExpirationHours <= 0 ? 72 : options.Value.ExpirationHours;
    }

    public int ExpirationHours { get; }
}
