using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class TenantMembershipRoleConfiguration : TenantEntityConfiguration<TenantMembershipRole>
{
    public override void Configure(EntityTypeBuilder<TenantMembershipRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("tenant_membership_roles");

        builder.Property(x => x.MembershipId)
            .HasColumnName("membership_id")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.HasIndex(x => new { x.MembershipId, x.RoleId })
            .IsUnique();

        builder.HasIndex(x => x.RoleId);

        builder.HasOne(x => x.Membership)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
