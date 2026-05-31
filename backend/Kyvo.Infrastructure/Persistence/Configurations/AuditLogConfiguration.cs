using Kyvo.Domain.Entities;
using Kyvo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kyvo.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : TenantEntityConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("audit_logs");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.MembershipId)
            .HasColumnName("membership_id");

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.ResourceType)
            .HasColumnName("resource_type")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.ResourceId)
            .HasColumnName("resource_id");

        builder.Property(x => x.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(120);

        builder.Property(x => x.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);
    }
}
