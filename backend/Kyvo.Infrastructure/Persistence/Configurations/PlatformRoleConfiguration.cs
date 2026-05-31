using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class PlatformRoleConfiguration : BaseEntityConfiguration<PlatformRole>
{
    public override void Configure(EntityTypeBuilder<PlatformRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("platform_roles");

        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .HasColumnName("is_system")
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .IsUnique();
    }
}
