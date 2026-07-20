using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;


namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectActivityLog : BaseEntity
{
    public Guid ProjectId { get; private set; }

    public Guid UserId { get; private set; }

    public string Action { get; private set; }

    public ActivityEntityType EntityType { get; private set; }

    public Guid? EntityId { get; private set; }

    public string? BeforeState { get; private set; }

    public string? AfterState { get; private set; }

    public new DateTimeOffset CreatedAt { get; private set; }

    public Project Project { get; private set; }

    private ProjectActivityLog() { }

    public static ProjectActivityLog Create(
        Guid projectId,
        Guid _UserId,
        string action,
        ActivityEntityType entityType,
        Guid? entityId = null,
        string? beforeState = null,
        string? afterState = null)
    {
        return new ProjectActivityLog
        {
            ProjectId = projectId,
            UserId = _UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeState = beforeState,
            AfterState = afterState,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
