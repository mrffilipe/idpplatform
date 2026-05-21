using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;

namespace IdPPlatform.Infrastructure.Services.Auth;

public sealed class ExternalLoginService : IExternalLoginService
{
    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IIdentityProviderTokenValidatorFactory _validatorFactory;
    private readonly IUserRepository _users;
    private readonly IExternalIdentityRepository _externalIdentities;
    private readonly ITenantMembershipRepository _memberships;
    private readonly IUserPlatformRoleRepository _userPlatformRoles;
    private readonly IUnitOfWork _unitOfWork;

    public ExternalLoginService(
        IIdentityProviderRepository identityProviders,
        IIdentityProviderTokenValidatorFactory validatorFactory,
        IUserRepository users,
        IExternalIdentityRepository externalIdentities,
        ITenantMembershipRepository memberships,
        IUserPlatformRoleRepository userPlatformRoles,
        IUnitOfWork unitOfWork)
    {
        _identityProviders = identityProviders;
        _validatorFactory = validatorFactory;
        _users = users;
        _externalIdentities = externalIdentities;
        _memberships = memberships;
        _userPlatformRoles = userPlatformRoles;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExternalLoginResult> LoginWithProviderAsync(
        string providerAlias,
        string identityToken,
        CancellationToken cancellationToken = default)
    {
        var provider = await _identityProviders.GetEnabledByAliasAsync(providerAlias, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.IdentityProvider.NotFound);

        if (provider.ProviderType == IdentityProviderType.Local)
        {
            throw new DomainBusinessRuleException(
                ApplicationErrorMessages.IdentityProvider.LocalNotAllowedForExternalLogin);
        }

        var validator = _validatorFactory.GetValidator(provider.ProviderType);
        var externalAuth = await validator.ValidateAsync(provider, identityToken, cancellationToken);

        var user = await _users.GetByEmailAsync(externalAuth.Email, cancellationToken);
        if (user is null)
        {
            user = new User(
                new EmailAddress(externalAuth.Email),
                externalAuth.Email.Split('@')[0]);
            await _users.AddAsync(user, cancellationToken);
        }

        var linkedIdentity = await _externalIdentities.GetByProviderAndProviderUserIdAsync(
            externalAuth.Provider,
            externalAuth.ProviderUserId,
            cancellationToken);

        if (linkedIdentity is null)
        {
            linkedIdentity = new ExternalIdentity(
                user.Id,
                externalAuth.Provider,
                externalAuth.ProviderUserId,
                externalAuth.Email);
            await _externalIdentities.AddAsync(linkedIdentity, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var memberships = await _memberships.ListByUserIdWithTenantAndRolesAsync(user.Id, cancellationToken);
        var platformRoleAssignments = await _userPlatformRoles.ListByUserIdAsync(user.Id, cancellationToken);
        var platformRoles = platformRoleAssignments.Select(x => x.Role.Key).ToList();

        return new ExternalLoginResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PlatformRoles = platformRoles,
            TenantMemberships = memberships
                .Select(m => new ExternalLoginTenantMembership
                {
                    TenantId = m.TenantId,
                    MembershipId = m.Id,
                    Roles = m.Roles.Select(r => r.Role.Key.Value).ToList()
                })
                .ToList()
        };
    }
}
