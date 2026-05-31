using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class TenantMembershipConfiguration : TenantEntityConfiguration<TenantMembership>
{
    public override void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        base.Configure(builder);

        builder.ToTable("tenant_memberships");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.TenantId });
    }
}
