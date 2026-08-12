namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class TaskAllocation
{
    public Guid TaskId { get; private set; }
    public Guid? EmployeeProfileId { get; private set; }
    public int AllocationPercentage { get; private set; }
    public string Status { get; private set; } = default!;
    public int? StoryPoints { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private TaskAllocation() { }

    public static TaskAllocation Create(
        Guid taskId,
        Guid? employeeProfileId,
        int allocationPercentage,
        string status,
        int? storyPoints,
        DateTimeOffset? completedAt,
        DateTimeOffset? deletedAt,
        bool isDeleted,
        int revision,
        DateTimeOffset occurredAt)
    {
        var allocation = new TaskAllocation { TaskId = taskId };
        allocation.Apply(employeeProfileId, allocationPercentage, status, storyPoints,
            completedAt, deletedAt, isDeleted, revision, occurredAt);
        return allocation;
    }

    public void Apply(
        Guid? employeeProfileId,
        int allocationPercentage,
        string status,
        int? storyPoints,
        DateTimeOffset? completedAt,
        DateTimeOffset? deletedAt,
        bool isDeleted,
        int revision,
        DateTimeOffset occurredAt)
    {
        EmployeeProfileId = employeeProfileId;
        AllocationPercentage = allocationPercentage;
        Status = status;
        StoryPoints = storyPoints;
        CompletedAt = completedAt;
        DeletedAt = deletedAt;
        IsDeleted = isDeleted;
        Revision = revision;
        OccurredAt = occurredAt;
    }

    public bool ContributesToAllocation =>
        EmployeeProfileId.HasValue &&
        AllocationPercentage > 0 &&
        !IsDeleted &&
        DeletedAt is null &&
        CompletedAt is null &&
        !string.Equals(Status, "Done", StringComparison.OrdinalIgnoreCase);
}
