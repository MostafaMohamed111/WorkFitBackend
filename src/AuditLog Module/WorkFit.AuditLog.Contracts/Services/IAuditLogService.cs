namespace WorkFit.AuditLog.Contracts.Services;

public interface IAuditLogService
{
    Task LogCreatedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default);
    Task LogUpdatedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default);
    Task LogDeletedAsync(Guid entityId, string entityType, Guid userId, string userDisplayName, string moduleName, CancellationToken cancellationToken = default);
}
