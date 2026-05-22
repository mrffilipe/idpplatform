namespace IdPPlatform.Application.Services.Registration;

/// <summary>
/// Public self-registration on the IdP page. Creates a User + UserCredential (BCrypt) but does NOT
/// create any tenant or membership; consumer applications subsequently call <c>/v1.0/auth/subscribe</c>
/// after the user signs in to attach a tenant/plan to the account.
/// </summary>
public interface IRegistrationService
{
    Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
