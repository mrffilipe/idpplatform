using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppEntity = Kyvo.Domain.Entities.Application;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class ApplicationConfiguration : BaseEntityConfiguration<AppEntity>
{
    public override void Configure(EntityTypeBuilder<AppEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("applications");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .HasColumnName("is_system")
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
