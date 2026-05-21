using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class TenantRoleConfiguration : TenantEntityConfiguration<TenantRole>
{
    public override void Configure(EntityTypeBuilder<TenantRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("tenant_roles");

        builder.OwnsOne(
            x => x.Key,
            b => b.Property(y => y.Value)
                .HasColumnName("key")
                .HasMaxLength(63)
                .IsRequired());

        builder.Navigation(x => x.Key)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.IsSystem)
            .HasColumnName("is_system")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}
