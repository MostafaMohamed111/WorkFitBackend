using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkFit.AuditLog.Domain.Entities;

namespace WorkFit.AuditLog.Infrastructure.Data.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.Action)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.UserDisplayName)
               .HasMaxLength(256)
               .IsRequired();

        builder.Property(x => x.ModuleName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasIndex(x => new { x.EntityId, x.EntityType });
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
