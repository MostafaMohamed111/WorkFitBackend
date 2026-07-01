
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectAssignment : BaseEntity
{
    public Guid ProjectId { get; private set; }

    public Guid EmployeeId { get; private set; }

    public string? RoleOnProject { get; private set; }

    public int AllocationPercentage { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public Project Project { get; private set; }
}
