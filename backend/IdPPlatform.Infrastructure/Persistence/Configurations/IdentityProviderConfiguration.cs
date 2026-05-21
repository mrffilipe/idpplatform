using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class IdentityProviderConfiguration : BaseEntityConfiguration<IdentityProvider>
{
    public override void Configure(EntityTypeBuilder<IdentityProvider> builder)
    {
        base.Configure(builder);

        builder.ToTable("identity_providers");

        builder.Property(x => x.Alias)
            .HasColumnName("alias")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.ProviderType)
            .HasColumnName("provider_type")
            .IsRequired();

        builder.Property(x => x.Enabled)
            .HasColumnName("enabled")
            .IsRequired();

        builder.Property(x => x.ConfigJson)
            .HasColumnName("config_json")
            .HasColumnType("json");

        builder.HasIndex(x => x.Alias)
            .IsUnique();
    }
}
