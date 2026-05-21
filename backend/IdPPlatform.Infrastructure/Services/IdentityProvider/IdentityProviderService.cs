using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Constants;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;

namespace IdPPlatform.Infrastructure.Services.IdentityProvider;

public sealed class IdentityProviderService : IIdentityProviderService
{
    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IUnitOfWork _unitOfWork;

    public IdentityProviderService(IIdentityProviderRepository identityProviders, IUnitOfWork unitOfWork)
    {
        _identityProviders = identityProviders;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> AddAsync(AddIdentityProviderRequest request, CancellationToken cancellationToken = default)
    {
        if (await _identityProviders.AliasAlreadyExistsAsync(request.Alias, cancellationToken))
        {
            throw new DomainBusinessRuleException(ApplicationErrorMessages.IdentityProvider.AliasAlreadyExists);
        }

        var provider = new Domain.Entities.IdentityProvider(
            request.Alias,
            request.DisplayName,
            request.ProviderType,
            enabled: true,
            request.ConfigJson);

        await _identityProviders.AddAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return provider.Id;
    }

    public async Task UpdateAsync(UpdateIdentityProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(request.Id, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        provider.UpdateDisplayName(request.DisplayName);
        provider.UpdateConfig(request.ConfigJson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task EnableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(id, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        provider.Enable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(id, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        if (provider.ProviderType == IdentityProviderType.Local
            && !await _identityProviders.AnyEnabledLocalProviderAsync(cancellationToken))
        {
            throw new DomainBusinessRuleException(
                ApplicationErrorMessages.IdentityProvider.CannotDisableLastLocalProvider);
        }

        provider.Disable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IdentityProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(id, cancellationToken);
        return provider is null ? null : MapToDto(provider);
    }

    public async Task<IReadOnlyList<IdentityProviderDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _identityProviders.ListAllAsync(cancellationToken);
        return providers.Select(MapToDto).ToList();
    }

    private static IdentityProviderDto MapToDto(Domain.Entities.IdentityProvider provider)
    {
        return new IdentityProviderDto
        {
            Id = provider.Id,
            Alias = provider.Alias,
            DisplayName = provider.DisplayName,
            ProviderType = provider.ProviderType,
            Enabled = provider.Enabled
        };
    }
}
