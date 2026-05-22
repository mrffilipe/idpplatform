using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Enums;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        var capabilityConverter = new ValueConverter<IReadOnlyCollection<IdpCapability>, int[]>(
            v => v.Select(c => (int)c).ToArray(),
            v => v.Select(c => (IdpCapability)c).ToList().AsReadOnly());

        // Comparer needed because EF Core does not know how to track changes on a custom collection.
        var capabilityComparer = new ValueComparer<IReadOnlyCollection<IdpCapability>>(
            (a, b) => (a ?? Array.Empty<IdpCapability>()).SequenceEqual(b ?? Array.Empty<IdpCapability>()),
            v => v.Aggregate(0, (hash, c) => HashCode.Combine(hash, c)),
            v => v.ToList().AsReadOnly());

        builder.Property(x => x.Capabilities)
            .HasColumnName("capabilities")
            .HasColumnType("integer[]")
            .HasConversion(capabilityConverter, capabilityComparer)
            .IsRequired();

        builder.HasIndex(x => x.Alias)
            .IsUnique();
    }
}
