using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

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

        // Store as PostgreSQL integer[]; List is required so Npgsql receives int[] from the converter
        // (IReadOnlyCollection breaks parameter binding with InvalidCastException).
        var capabilityComparer = new ValueComparer<List<IdpCapability>>(
            (left, right) => (left ?? new List<IdpCapability>()).SequenceEqual(right ?? new List<IdpCapability>()),
            capabilities => capabilities.Aggregate(0, (hash, c) => HashCode.Combine(hash, (int)c)),
            capabilities => capabilities.ToList());

        builder.Property(x => x.Capabilities)
            .HasColumnName("capabilities")
            .HasColumnType("integer[]")
            .HasConversion(
                capabilities => capabilities.Select(c => (int)c).ToArray(),
                values => values.Select(v => (IdpCapability)v).ToList(),
                capabilityComparer)
            .IsRequired();

        builder.HasIndex(x => x.Alias)
            .IsUnique();
    }
}
