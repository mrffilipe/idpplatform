using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class OidcAuthorizationCodeConfiguration : BaseEntityConfiguration<OidcAuthorizationCode>
{
    public override void Configure(EntityTypeBuilder<OidcAuthorizationCode> builder)
    {
        base.Configure(builder);

        builder.ToTable("oidc_authorization_codes");

        builder.Property(x => x.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ApplicationClientId)
            .HasColumnName("application_client_id")
            .IsRequired();

        builder.Property(x => x.AuthSessionId)
            .HasColumnName("auth_session_id")
            .IsRequired();

        builder.Property(x => x.RedirectUri)
            .HasColumnName("redirect_uri")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Scopes)
            .HasColumnName("scopes")
            .HasColumnType("json")
            .IsRequired();

        builder.Property(x => x.CodeChallenge)
            .HasColumnName("code_challenge")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.CodeChallengeMethod)
            .HasColumnName("code_challenge_method")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Nonce)
            .HasColumnName("nonce")
            .HasMaxLength(200);

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(x => x.ConsumedAt)
            .HasColumnName("consumed_at");

        builder.HasIndex(x => x.CodeHash)
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
