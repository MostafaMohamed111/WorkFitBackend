using Microsoft.EntityFrameworkCore;
using WorkFit.AuditLog.Domain.Entities;

namespace WorkFit.AuditLog.Infrastructure.Data;

public sealed class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options) : base(options) { }

    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("AuditLog");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditLogDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
