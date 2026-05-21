using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class OidcRefreshTokenConfiguration : BaseEntityConfiguration<OidcRefreshToken>
{
    public override void Configure(EntityTypeBuilder<OidcRefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("oidc_refresh_tokens");

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ApplicationClientId)
            .HasColumnName("application_client_id")
            .IsRequired();

        builder.Property(x => x.AuthSessionId)
            .HasColumnName("auth_session_id")
            .IsRequired();

        builder.Property(x => x.Scopes)
            .HasColumnName("scopes")
            .HasColumnType("json")
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasOne(x => x.ApplicationClient)
            .WithMany()
            .HasForeignKey(x => x.ApplicationClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AuthSession)
            .WithMany()
            .HasForeignKey(x => x.AuthSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
