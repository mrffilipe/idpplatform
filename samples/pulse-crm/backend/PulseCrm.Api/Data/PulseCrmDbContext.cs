using Microsoft.EntityFrameworkCore;
using TenancyKit.Abstractions;
using TenancyKit.Core;
using TenancyKit.EntityFrameworkCore;
using IdPPlatform.AspNetCore.TenancyKit;

namespace PulseCrm.Api.Data;

public sealed class PulseCrmDbContext : DbContext
{
    private readonly TenancyKitOptions<ProductTenantInfo> _tenancyOptions;
    private readonly ITenantContextAccessor<ProductTenantInfo> _tenantContextAccessor;

    public PulseCrmDbContext(
        DbContextOptions<PulseCrmDbContext> options,
        TenancyKitOptions<ProductTenantInfo> tenancyOptions,
        ITenantContextAccessor<ProductTenantInfo> tenantContextAccessor)
        : base(options)
    {
        _tenancyOptions = tenancyOptions;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.Property(x => x.CompanyName).HasMaxLength(200);
            entity.Property(x => x.TenantKey).HasMaxLength(80);
            entity.Property(x => x.PlanCode).HasMaxLength(80);
            entity.Property(x => x.ExternalCustomerId).HasMaxLength(120);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TenantId);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.Phone).HasMaxLength(40);
        });

        modelBuilder.ApplyMultiTenancy(_tenancyOptions, _tenantContextAccessor);
    }
}
