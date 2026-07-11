using WorkFit.AuditLog.Contracts.Services;
using WorkFit.AuditLog.Domain.Entities;
using WorkFit.AuditLog.Infrastructure.Data;

namespace WorkFit.AuditLog.CrossModule;

public sealed class AuditLogService : IAuditLogService
{
    private readonly AuditLogDbContext _context;

    public AuditLogService(AuditLogDbContext context) => _context = context;

    public async Task LogCreatedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default)
    {
        var log = AuditLogEntry.Create(entityId, entityType, "Created", userId, userDisplayName, moduleName);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogUpdatedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default)
    {
        var log = AuditLogEntry.Create(entityId, entityType, "Updated", userId, userDisplayName, moduleName);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogDeletedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default)
    {
        var log = AuditLogEntry.Create(entityId, entityType, "Deleted", userId, userDisplayName, moduleName);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
