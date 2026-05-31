namespace Kyvo.Application.Services.LocalAuthentication;

public interface ILocalAuthenticationService
{
    Task<LocalLoginResult?> LoginAsync(
        LocalLoginRequest request,
        CancellationToken cancellationToken = default);
}
