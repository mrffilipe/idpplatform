using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Kyvo.Application.Services.Email;
using Kyvo.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Services.Email;

public sealed class AwsSesEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly IAmazonSimpleEmailServiceV2 _ses;

    public AwsSesEmailService(IOptions<EmailOptions> options, IAmazonSimpleEmailServiceV2 ses)
    {
        _options = options.Value;
        _ses = ses;
    }

    public Task SendInviteAsync(
        string toEmail,
        string tenantName,
        string inviteToken,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Invite to {tenantName}";
        var body = $"You were invited to join {tenantName}. Use this token to accept the invite: {inviteToken}";

        var request = new SendEmailRequest
        {
            FromEmailAddress = _options.FromAddress,
            Destination = new Destination
            {
                ToAddresses = [toEmail]
            },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Data = subject },
                    Body = new Body
                    {
                        Text = new Content { Data = body }
                    }
                }
            }
        };

        return _ses.SendEmailAsync(request, cancellationToken);
    }
}
