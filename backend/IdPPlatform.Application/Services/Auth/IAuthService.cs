namespace IdPPlatform.Application.Services.Auth;

public interface IAuthService
{
    Task<TenantContextResult> SwitchTenantAsync(
        SwitchTenantRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Onboarding SaaS: cria tenant vinculado à Application da sessão OAuth atual (sem expor applicationId ao client).
    /// </summary>
    Task<TenantContextResult> SubscribeTenantAsync(
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSessionDto>> ListActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
