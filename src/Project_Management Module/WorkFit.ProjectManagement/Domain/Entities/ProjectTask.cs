using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskType TaskType { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CreatedById { get; private set; }
    public int? StoryPoints { get; private set; }
    public int AllocationPercentage { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string? SourceSystem { get; private set; }
    public string? SourceReferenceId { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsActive => Status != TaskStatus.Done && AllocationPercentage > 0;


    public Project Project { get; private set; } = default!;

    private ProjectTask() { }

    public static ProjectTask Create(
        Guid projectId,
        string title,
        string? description,
        TaskType taskType,
        TaskPriority priority,
        Guid createdById,
        Guid? assigneeId,
        int? storyPoints,
        DateOnly? dueDate,
        int allocationPercentage = 0)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 100)
            throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "ProjectTask", "Title");

        if (allocationPercentage < 0 || allocationPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(allocationPercentage),
                "Allocation percentage must be between 0 and 100.");
        

        return new ProjectTask
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            TaskType = taskType,
            Priority = priority,
            Status = TaskStatus.ToDo,
            CreatedById = createdById,
            AssigneeId = assigneeId,
            StoryPoints = storyPoints,
            AllocationPercentage = allocationPercentage,
            DueDate = dueDate
        };
    }

    public void SetAllocationPercentage(int allocationPercentage)
    {
        if (Status == TaskStatus.Done)
            return;

        if (allocationPercentage < 0 || allocationPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(allocationPercentage),
                "Allocation percentage must be between 0 and 100.");

        AllocationPercentage = allocationPercentage;
        MarkUpdated();
    }

    public void UpdateDetails(string? title, string? description, TaskPriority? priority,
        int? storyPoints, DateOnly? dueDate)
    {
        if (title is not null)
        {
            if (title.Length < 3 || title.Length > 100)
                throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "ProjectTask", "Title");
            Title = title;
        }

        if (description is not null) Description = description;
        if (priority.HasValue) Priority = priority.Value;
        if (storyPoints.HasValue) StoryPoints = storyPoints.Value;

        if (dueDate.HasValue)
        {
            DueDate = dueDate.Value;
        }

        MarkUpdated();
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        if (Status == TaskStatus.Done && newStatus != TaskStatus.Done)
            throw new TaskAlreadyDoneException(ModuleMarker.ModuleName);

        Status = newStatus;
        MarkUpdated();
    }

    public void Assign(Guid assigneeId)
    {
        if (assigneeId == Guid.Empty)
            throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, "ProjectTask", "AssigneeId");

        AssigneeId = assigneeId;
        MarkUpdated();
    }

    public void Complete()
    {
        if (Status == TaskStatus.Done)
            throw new TaskAlreadyDoneException(ModuleMarker.ModuleName);

        Status = TaskStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    public void Delete()
    {
        if (Status == TaskStatus.Done)
            throw new CannotDeleteCompletedTaskException(Id);

        if (IsDeleted)
            throw new TaskAlreadyDeletedException(Id);

        MarkDeleted();
        DeletedAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    /// <summary>
    /// Records the external system and reference identifier this task was synced from.
    /// Used by the Integration layer to enable idempotent re-sync.
    /// </summary>
    public void SetSource(string sourceSystem, string sourceReferenceId)
    {
        SourceSystem      = sourceSystem;
        SourceReferenceId = sourceReferenceId;
        MarkUpdated();
    }
}