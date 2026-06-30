using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;


namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectActivityLog : BaseEntity
{
    public Guid ProjectId { get; private set; }

    public Guid ActorId { get; private set; }

    public string Action { get; private set; }

    public ActivityEntityType EntityType { get; private set; }

    public Guid? EntityId { get; private set; }

    public string? BeforeState { get; private set; }

    public string? AfterState { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Project Project { get; private set; }
}
