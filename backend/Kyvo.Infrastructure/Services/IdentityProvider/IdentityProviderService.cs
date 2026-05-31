using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.Repositories;

namespace Kyvo.Infrastructure.Services.IdentityProvider;

public sealed class IdentityProviderService : IIdentityProviderService
{
    private static readonly IReadOnlyCollection<IdpCapability> SocialCapabilities =
    [
        IdpCapability.GoogleSocial,
        IdpCapability.MicrosoftSocial,
        IdpCapability.AppleSocial
    ];

    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IIdentityProviderConfigValidator _configValidator;
    private readonly IIdentityProviderConfigCipher _configCipher;
    private readonly IUnitOfWork _unitOfWork;

    public IdentityProviderService(
        IIdentityProviderRepository identityProviders,
        IIdentityProviderConfigValidator configValidator,
        IIdentityProviderConfigCipher configCipher,
        IUnitOfWork unitOfWork)
    {
        _identityProviders = identityProviders;
        _configValidator = configValidator;
        _configCipher = configCipher;
        _unitOfWork = unitOfWork;
    }

    public async Task<AddIdentityProviderResult> AddAsync(AddIdentityProviderRequest request, CancellationToken cancellationToken = default)
    {
        if (await _identityProviders.AliasAlreadyExistsAsync(request.Alias, cancellationToken))
        {
            throw new DomainBusinessRuleException(ApplicationErrorMessages.IdentityProvider.AliasAlreadyExists);
        }

        _configValidator.ValidateForSave(request.ProviderType, request.ConfigJson);
        await EnforceCapabilityUniquenessOnAddAsync(request.Capabilities, cancellationToken);

        // Encrypt sensitive fields before persisting so secrets never live in plain text at rest.
        var encryptedConfig = _configCipher.Encrypt(request.ProviderType, request.ConfigJson);

        var provider = new Domain.Entities.IdentityProvider(
            request.Alias,
            request.DisplayName,
            request.ProviderType,
            request.Capabilities,
            enabled: true,
            configJson: encryptedConfig);

        await _identityProviders.AddAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var warnings = await BuildSocialWarningsAsync(provider.Id, request.Capabilities, cancellationToken);
        return new AddIdentityProviderResult { Id = provider.Id, Warnings = warnings };
    }

    public async Task UpdateAsync(UpdateIdentityProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(request.Id, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        provider.UpdateDisplayName(request.DisplayName);

        if (request.Capabilities is not null)
        {
            provider.UpdateCapabilities(request.Capabilities);
        }

        if (request.ConfigJson is not null)
        {
            _configValidator.ValidateForSave(provider.ProviderType, request.ConfigJson);
            var encryptedConfig = _configCipher.Encrypt(provider.ProviderType, request.ConfigJson);
            provider.UpdateConfig(encryptedConfig);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task EnableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetForUpdateAsync(id, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        if (provider.Capabilities.Contains(IdpCapability.LocalPassword))
        {
            var conflicts = await _identityProviders.ListEnabledByCapabilityAsync(IdpCapability.LocalPassword, cancellationToken);
            if (conflicts.Any(x => x.Id != provider.Id))
            {
                throw new DomainBusinessRuleException(
                    ApplicationErrorMessages.IdentityProviderCapability.LocalPasswordAlreadyHandled);
            }
        }

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

    private async Task EnforceCapabilityUniquenessOnAddAsync(
        IReadOnlyCollection<IdpCapability> capabilities,
        CancellationToken cancellationToken)
    {
        if (!capabilities.Contains(IdpCapability.LocalPassword))
        {
            return;
        }

        var existing = await _identityProviders.ListEnabledByCapabilityAsync(IdpCapability.LocalPassword, cancellationToken);
        if (existing.Count > 0)
        {
            throw new DomainBusinessRuleException(
                ApplicationErrorMessages.IdentityProviderCapability.LocalPasswordAlreadyHandled);
        }
    }

    private async Task<IReadOnlyList<string>> BuildSocialWarningsAsync(
        Guid newProviderId,
        IReadOnlyCollection<IdpCapability> capabilities,
        CancellationToken cancellationToken)
    {
        var warnings = new List<string>();

        foreach (var capability in capabilities.Where(SocialCapabilities.Contains))
        {
            var others = await _identityProviders.ListEnabledByCapabilityAsync(capability, cancellationToken);
            var collisions = others.Where(x => x.Id != newProviderId).Select(x => x.Alias).ToList();
            if (collisions.Count > 0)
            {
                warnings.Add(string.Format(
                    ApplicationErrorMessages.IdentityProviderCapability.SocialAlreadyHandledFormat,
                    capability,
                    string.Join(", ", collisions)));
            }
        }

        return warnings;
    }

    private static IdentityProviderDto MapToDto(Domain.Entities.IdentityProvider provider)
    {
        return new IdentityProviderDto
        {
            Id = provider.Id,
            Alias = provider.Alias,
            DisplayName = provider.DisplayName,
            ProviderType = provider.ProviderType,
            Enabled = provider.Enabled,
            Capabilities = provider.Capabilities
        };
    }
}
