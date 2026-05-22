namespace IdPPlatform.Application.Services.Auth;

public interface IAuthService
{
    Task<TenantContextResult> SwitchTenantAsync(SwitchTenantRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// SaaS onboarding: creates a tenant linked to the Application of the current OAuth session (without exposing applicationId to the client).
    /// </summary>
    Task<TenantContextResult> SubscribeTenantAsync(SubscribeTenantRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSessionDto>> ListActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
