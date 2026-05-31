using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class TenantInviteConfiguration : TenantEntityConfiguration<TenantInvite>
{
    public override void Configure(EntityTypeBuilder<TenantInvite> builder)
    {
        base.Configure(builder);

        builder.ToTable("tenant_invites");

        builder.OwnsOne(
            x => x.Email,
            b => b.Property(y => y.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired());

        builder.Navigation(x => x.Email)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.ConsumedAt)
            .HasColumnName("consumed_at");

        builder.Property(x => x.InvitedByUserId)
            .HasColumnName("invited_by_user_id")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();
    }
}
