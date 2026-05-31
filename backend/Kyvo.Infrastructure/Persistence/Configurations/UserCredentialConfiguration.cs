using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class UserCredentialConfiguration : BaseEntityConfiguration<UserCredential>
{
    public override void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_credentials");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Credentials)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
