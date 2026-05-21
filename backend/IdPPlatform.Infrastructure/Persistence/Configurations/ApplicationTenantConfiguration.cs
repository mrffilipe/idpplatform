using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class ApplicationTenantConfiguration : BaseEntityConfiguration<ApplicationTenant>
{
    public override void Configure(EntityTypeBuilder<ApplicationTenant> builder)
    {
        base.Configure(builder);

        builder.ToTable("application_tenants");

        builder.Property(x => x.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.ExternalCustomerId)
            .HasColumnName("external_customer_id")
            .HasMaxLength(255);

        builder.Property(x => x.PlanCode)
            .HasColumnName("plan_code")
            .HasMaxLength(120);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(x => new { x.ApplicationId, x.TenantId })
            .IsUnique();

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Tenants)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
