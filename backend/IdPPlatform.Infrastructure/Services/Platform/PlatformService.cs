using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.Platform;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Constants;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;
using IdPPlatform.Application.Services.Oidc;
using IdPPlatform.Infrastructure.Configurations;
using IdPPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

// Note: audit log is not created during bootstrap because AuditLog is a TenantEntity
// and requires a TenantId — bootstrap predates any tenant. The bootstrap timestamp
// is already tracked via PlatformConfiguration.BootstrappedAt.

namespace IdPPlatform.Infrastructure.Services.Platform;

public sealed class PlatformService : IPlatformService
{
    private readonly IPlatformConfigurationRepository _platformConfigurations;
    private readonly IUserRepository _users;
    private readonly IUserCredentialRepository _userCredentials;
    private readonly IUserPlatformRoleRepository _userPlatformRoles;
    private readonly IPlatformRoleRepository _platformRoles;
    private readonly IIdentityProviderRepository _identityProviders;
    private readonly IApplicationRepository _applications;
    private readonly IApplicationClientRepository _clients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly BootstrapOptions _bootstrapOptions;

    public PlatformService(
        IPlatformConfigurationRepository platformConfigurations,
        IUserRepository users,
        IUserCredentialRepository userCredentials,
        IUserPlatformRoleRepository userPlatformRoles,
        IPlatformRoleRepository platformRoles,
        IIdentityProviderRepository identityProviders,
        IApplicationRepository applications,
        IApplicationClientRepository clients,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        IOptions<BootstrapOptions> bootstrapOptions)
    {
        _platformConfigurations = platformConfigurations;
        _users = users;
        _userCredentials = userCredentials;
        _userPlatformRoles = userPlatformRoles;
        _platformRoles = platformRoles;
        _identityProviders = identityProviders;
        _applications = applications;
        _clients = clients;
        _unitOfWork = unitOfWork;
        _context = context;
        _bootstrapOptions = bootstrapOptions.Value;
    }

    public async Task<BootstrapResult> BootstrapAsync(
        BootstrapRequest request,
        CancellationToken cancellationToken = default)
    {
        var adminEmail = _bootstrapOptions.AdminEmail;
        var adminPassword = _bootstrapOptions.AdminPassword;

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new DomainBusinessRuleException(
                ApplicationErrorMessages.Auth.PlatformBootstrapAdminCredentialsNotConfigured);
        }

        BootstrapResult? result = null;

        await _unitOfWork.ExecuteInSerializableTransactionAsync(async transactionCt =>
        {
            var configuration = await _platformConfigurations.GetForUpdateAsync(transactionCt);
            if (configuration?.IsBootstrapped == true)
            {
                throw new DomainBusinessRuleException(
                    ApplicationErrorMessages.Auth.PlatformBootstrapAlreadyCompleted);
            }

            if (await _applications.SlugAlreadyExistsAsync(
                PlatformDefaults.AdminConsole.ApplicationSlug,
                transactionCt))
            {
                throw new DomainBusinessRuleException(
                    ApplicationErrorMessages.Auth.PlatformBootstrapApplicationSlugAlreadyExists);
            }

            if (await _clients.GetByClientIdAsync(PlatformDefaults.AdminConsole.ClientId, transactionCt) is not null)
            {
                throw new DomainBusinessRuleException(
                    ApplicationErrorMessages.Auth.PlatformBootstrapClientIdAlreadyExists);
            }

            var user = await _users.GetByEmailAsync(adminEmail.Trim(), transactionCt);
            if (user is null)
            {
                var displayName = string.IsNullOrWhiteSpace(_bootstrapOptions.AdminDisplayName)
                    ? adminEmail.Split('@')[0]
                    : _bootstrapOptions.AdminDisplayName.Trim();

                user = new User(new EmailAddress(adminEmail.Trim()), displayName);
                await _users.AddAsync(user, transactionCt);
            }

            var credential = await _userCredentials.GetByUserIdAsync(user.Id, transactionCt);
            if (credential is null)
            {
                credential = new UserCredential(user.Id, BCrypt.Net.BCrypt.HashPassword(adminPassword));
                await _userCredentials.AddAsync(credential, transactionCt);
            }

            var platAdminRole = await _platformRoles.GetByKeyAsync(
                PlatformRoleDefaults.PlatformAdministrator,
                transactionCt);

            if (platAdminRole is null)
            {
                platAdminRole = new PlatformRole(
                    PlatformRoleDefaults.PlatformAdministrator,
                    "Platform Administrator",
                    isSystem: true);
                await _platformRoles.AddAsync(platAdminRole, transactionCt);
            }

            if (!await _userPlatformRoles.ExistsAsync(user.Id, platAdminRole.Id, transactionCt))
            {
                await _userPlatformRoles.AddAsync(
                    new UserPlatformRole(user.Id, platAdminRole.Id),
                    transactionCt);
            }

            var localIdp = await _identityProviders.GetByAliasAsync(
                PlatformDefaults.LocalIdentityProvider.Alias,
                transactionCt);

            if (localIdp is null)
            {
                localIdp = new Domain.Entities.IdentityProvider(
                    PlatformDefaults.LocalIdentityProvider.Alias,
                    PlatformDefaults.LocalIdentityProvider.DisplayName,
                    IdentityProviderType.Local,
                    enabled: true);
                await _identityProviders.AddAsync(localIdp, transactionCt);
            }

            var application = new Domain.Entities.Application(
                PlatformDefaults.AdminConsole.ApplicationName,
                PlatformDefaults.AdminConsole.ApplicationSlug,
                ApplicationType.Web,
                isSystem: true);
            await _applications.AddAsync(application, transactionCt);

            var client = new ApplicationClient(
                application.Id,
                PlatformDefaults.AdminConsole.ClientId,
                clientSecretHash: null,
                ClientType.Public,
                JsonSerializer.Serialize(PlatformDefaults.AdminConsole.DefaultRedirectUris),
                JsonSerializer.Serialize(PlatformDefaults.AdminConsole.AllowedScopes),
                accessTokenTtlSeconds: 900,
                isSystem: true);
            await _clients.AddAsync(client, transactionCt);

            if (configuration is null)
            {
                configuration = new PlatformConfiguration();
                await _platformConfigurations.AddAsync(configuration, transactionCt);
            }

            configuration.MarkBootstrapped(user.Id, client.ClientId);

            await _unitOfWork.SaveChangesAsync(transactionCt);

            result = new BootstrapResult
            {
                IsConfigured = true,
                RootUserId = user.Id,
                OauthClientId = client.ClientId
            };
        }, cancellationToken);

        return result!;
    }

    public async Task<PlatformStatusResult> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _context.PlatformConfigurations
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var isConfigured = configuration?.IsBootstrapped == true && configuration.RootUserId.HasValue;

        if (isConfigured)
        {
            await EnsureAdminConsoleOfflineAccessScopeAsync(cancellationToken);
        }

        return new PlatformStatusResult
        {
            IsConfigured = isConfigured,
            RequiresBootstrap = !isConfigured,
            OauthClientId = isConfigured ? configuration?.OauthClientId : null
        };
    }

    /// <summary>
    /// Installations bootstrapped before <c>offline_access</c> was added to the admin console need the scope for refresh tokens (SPA).
    /// </summary>
    private async Task EnsureAdminConsoleOfflineAccessScopeAsync(CancellationToken cancellationToken)
    {
        var client = await _clients.GetByClientIdAsync(PlatformDefaults.AdminConsole.ClientId, cancellationToken);
        if (client is null || !client.IsSystem)
        {
            return;
        }

        var scopes = JsonSerializer.Deserialize<List<string>>(client.AllowedScopes) ?? [];
        if (scopes.Contains(OidcConstants.Scopes.OfflineAccess, StringComparer.Ordinal))
        {
            return;
        }

        scopes.Add(OidcConstants.Scopes.OfflineAccess);
        var updatedScopes = JsonSerializer.Serialize(scopes);

        await _context.ApplicationClients
            .Where(c => c.Id == client.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.AllowedScopes, updatedScopes),
                cancellationToken);
    }
}
