using IdPPlatform.Domain.Entities;
using IdPPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdPPlatform.Infrastructure.Persistence.Configurations;

public sealed class ApplicationClientConfiguration : BaseEntityConfiguration<ApplicationClient>
{
    public override void Configure(EntityTypeBuilder<ApplicationClient> builder)
    {
        base.Configure(builder);

        builder.ToTable("application_clients");

        builder.Property(x => x.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ClientSecretHash)
            .HasColumnName("client_secret_hash")
            .HasMaxLength(500);

        builder.Property(x => x.ClientType)
            .HasColumnName("client_type")
            .IsRequired();

        builder.Property(x => x.RedirectUris)
            .HasColumnName("redirect_uris")
            .HasColumnType("json")
            .IsRequired();

        builder.Property(x => x.AllowedScopes)
            .HasColumnName("allowed_scopes")
            .HasColumnType("json")
            .IsRequired();

        builder.Property(x => x.AccessTokenTtlSeconds)
            .HasColumnName("access_token_ttl_seconds")
            .IsRequired();

        builder.Property(x => x.IsSystem)
            .HasColumnName("is_system")
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.ClientId)
            .IsUnique();

        builder.HasOne(x => x.Application)
            .WithMany(x => x.Clients)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
