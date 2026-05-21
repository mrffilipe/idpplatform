using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class PlatformConfigurationConfiguration : BaseEntityConfiguration<PlatformConfiguration>
{
    public override void Configure(EntityTypeBuilder<PlatformConfiguration> builder)
    {
        base.Configure(builder);

        builder.ToTable("platform_configurations");

        builder.Property(x => x.IsBootstrapped)
            .HasColumnName("is_bootstrapped")
            .IsRequired();

        builder.Property(x => x.RootUserId)
            .HasColumnName("root_user_id");

        builder.Property(x => x.OauthClientId)
            .HasColumnName("oauth_client_id")
            .HasMaxLength(255);

        builder.Property(x => x.BootstrappedAt)
            .HasColumnName("bootstrapped_at");
    }
}
