using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class UserPlatformRoleConfiguration : BaseEntityConfiguration<UserPlatformRole>
{
    public override void Configure(EntityTypeBuilder<UserPlatformRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_platform_roles");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.RoleId })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.PlatformRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserAssignments)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
