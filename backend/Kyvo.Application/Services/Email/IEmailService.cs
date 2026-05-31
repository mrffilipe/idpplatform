namespace Kyvo.Application.Services.Email;

public interface IEmailService
{
    Task SendInviteAsync(
        string toEmail,
        string tenantName,
        string inviteToken,
        CancellationToken cancellationToken = default);
}
