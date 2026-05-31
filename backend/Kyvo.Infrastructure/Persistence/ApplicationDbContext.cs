using Kyvo.Domain.Common;
using Kyvo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TenancyKit.Abstractions;
using TenancyKit.Core;
using TenancyKit.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly TenancyKitOptions<TenantInfoAdapter> _tenancyOptions;
    private readonly ITenantContextAccessor<TenantInfoAdapter> _tenantContextAccessor;

    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<UserCredential> UserCredentials { get; private set; } = null!;
    public DbSet<UserPlatformRole> UserPlatformRoles { get; private set; } = null!;
    public DbSet<PlatformRole> PlatformRoles { get; private set; } = null!;
    public DbSet<ExternalIdentity> ExternalIdentities { get; private set; } = null!;
    public DbSet<IdentityProvider> IdentityProviders { get; private set; } = null!;
    public DbSet<Tenant> Tenants { get; private set; } = null!;
    public DbSet<TenantRole> TenantRoles { get; private set; } = null!;
    public DbSet<TenantMembership> TenantMemberships { get; private set; } = null!;
    public DbSet<TenantMembershipRole> TenantMembershipRoles { get; private set; } = null!;
    public DbSet<Domain.Entities.Application> Applications { get; private set; } = null!;
    public DbSet<ApplicationClient> ApplicationClients { get; private set; } = null!;
    public DbSet<ApplicationTenant> ApplicationTenants { get; private set; } = null!;
    public DbSet<AuthSession> AuthSessions { get; private set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; private set; } = null!;
    public DbSet<TenantInvite> TenantInvites { get; private set; } = null!;
    public DbSet<TenantInviteRole> TenantInviteRoles { get; private set; } = null!;
    public DbSet<PlatformConfiguration> PlatformConfigurations { get; private set; } = null!;
    public DbSet<OidcAuthorizationCode> OidcAuthorizationCodes { get; private set; } = null!;
    public DbSet<OidcRefreshToken> OidcRefreshTokens { get; private set; } = null!;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        TenancyKitOptions<TenantInfoAdapter> tenancyOptions,
        ITenantContextAccessor<TenantInfoAdapter> tenantContextAccessor) : base(options)
    {
        _tenancyOptions = tenancyOptions;
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyMultiTenancy(_tenancyOptions, _tenantContextAccessor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAt();
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedAt();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
