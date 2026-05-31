using Kyvo.Application.Exceptions;
using Kyvo.Application.Services.Platform;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Constants;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.Repositories;
using Kyvo.Domain.ValueObjects;
using Kyvo.Application.Services.Oidc;
using Kyvo.Infrastructure.Configurations;
using Kyvo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

// Note: audit log is not created during bootstrap because AuditLog is a TenantEntity
// and requires a TenantId — bootstrap predates any tenant. The bootstrap timestamp
// is already tracked via PlatformConfiguration.BootstrappedAt.

namespace Kyvo.Infrastructure.Services.Platform;

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
    private readonly JwtOptions _jwtOptions;

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
        IOptions<BootstrapOptions> bootstrapOptions,
        IOptions<JwtOptions> jwtOptions)
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
        _jwtOptions = jwtOptions.Value;
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
                    new[] { IdpCapability.LocalPassword },
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
                JsonSerializer.Serialize(BuildAdminConsoleRedirectUris()),
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
            await EnsureAdminConsoleRedirectUriAsync(cancellationToken);
        }

        return new PlatformStatusResult
        {
            IsConfigured = isConfigured,
            RequiresBootstrap = !isConfigured,
            OauthClientId = isConfigured ? configuration?.OauthClientId : null
        };
    }

    private IReadOnlyList<string> BuildAdminConsoleRedirectUris()
    {
        var uris = new HashSet<string>(StringComparer.Ordinal);
        foreach (var uri in PlatformDefaults.AdminConsole.DefaultRedirectUris)
        {
            uris.Add(uri);
        }

        var issuer = _jwtOptions.Issuer.Trim().TrimEnd('/');
        if (!string.IsNullOrEmpty(issuer))
        {
            uris.Add($"{issuer}/auth/callback");
        }

        return uris.ToList();
    }

    /// <summary>
    /// Ensures the admin SPA redirect for the configured JWT issuer (monolith / same-origin deploy).
    /// </summary>
    private async Task EnsureAdminConsoleRedirectUriAsync(CancellationToken cancellationToken)
    {
        var client = await _clients.GetByClientIdAsync(PlatformDefaults.AdminConsole.ClientId, cancellationToken);
        if (client is null || !client.IsSystem)
        {
            return;
        }

        var expected = BuildAdminConsoleRedirectUris();
        var current = JsonSerializer.Deserialize<List<string>>(client.RedirectUris) ?? [];
        if (expected.All(uri => current.Contains(uri, StringComparer.Ordinal)))
        {
            return;
        }

        var merged = new HashSet<string>(current, StringComparer.Ordinal);
        foreach (var uri in expected)
        {
            merged.Add(uri);
        }

        var updated = JsonSerializer.Serialize(merged.ToList());

        await _context.ApplicationClients
            .Where(c => c.Id == client.Id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.RedirectUris, updated),
                cancellationToken);
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
