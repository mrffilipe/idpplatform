using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class TenantInviteRoleConfiguration : TenantEntityConfiguration<TenantInviteRole>
{
    public override void Configure(EntityTypeBuilder<TenantInviteRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("tenant_invite_roles");

        builder.Property(x => x.InviteId)
            .HasColumnName("invite_id")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.HasIndex(x => new { x.InviteId, x.RoleId })
            .IsUnique();

        builder.HasIndex(x => x.RoleId);

        builder.HasOne(x => x.Invite)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.InviteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
