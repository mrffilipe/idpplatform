using Kyvo.Application.Interfaces;
using Kyvo.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Services.Invite;

public sealed class InvitePolicy : IInvitePolicy
{
    public InvitePolicy(IOptions<InviteOptions> options)
    {
        ExpirationHours = options.Value.ExpirationHours <= 0 ? 72 : options.Value.ExpirationHours;
    }

    public int ExpirationHours { get; }
}
