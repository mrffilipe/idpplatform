namespace Kyvo.Application.Services.IdentityProvider;

public interface IIdentityProviderService
{
    Task<AddIdentityProviderResult> AddAsync(AddIdentityProviderRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateIdentityProviderRequest request, CancellationToken cancellationToken = default);

    Task EnableAsync(Guid id, CancellationToken cancellationToken = default);

    Task DisableAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IdentityProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IdentityProviderDto>> ListAsync(CancellationToken cancellationToken = default);
}
