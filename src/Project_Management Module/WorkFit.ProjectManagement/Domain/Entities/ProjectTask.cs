using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public Guid ProjectId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TaskType TaskType { get; private set; }

    public TaskStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public Guid CreatedById { get; private set; }

    public int? StoryPoints { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public string? SourceSystem { get; private set; }

    public string? SourceReferenceId { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public Project Project { get; private set; }
}
