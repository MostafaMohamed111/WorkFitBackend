using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.AuditLog.Domain.Entities;

public class AuditLogEntry : BaseEntity
{
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; }
    public string Action { get; private set; }
    public Guid UserId { get; private set; }
    public string UserDisplayName { get; private set; }
    public string ModuleName { get; private set; }
    public new DateTimeOffset CreatedAt { get; private set; }

    private AuditLogEntry() { }

    public static AuditLogEntry Create(
        Guid entityId,
        string entityType,
        string action,
        Guid userId,
        string userDisplayName,
        string moduleName)
    {
        return new AuditLogEntry
        {
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            UserId = userId,
            UserDisplayName = userDisplayName,
            ModuleName = moduleName,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
