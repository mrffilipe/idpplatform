using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class ExternalIdentityConfiguration : BaseEntityConfiguration<ExternalIdentity>
{
    public override void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        base.Configure(builder);

        builder.ToTable("external_identities");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasColumnName("provider")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasColumnName("provider_user_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.OwnsOne(
            x => x.Email,
            b => b.Property(y => y.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired());

        builder.Navigation(x => x.Email)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();
    }
}
