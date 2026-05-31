using Kyvo.Domain.Entities;
using Kyvo.Domain.ValueObjects;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");

        builder.OwnsOne(
            x => x.Email,
            b => b.Property(y => y.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired());

        builder.Navigation(x => x.Email)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.OwnsOne(
            x => x.PhotoUrl,
            b => b.Property(y => y.Value)
                .HasColumnName("photo_url")
                .HasMaxLength(PhotoUrl.MaxLength));

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();
    }
}
