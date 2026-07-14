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

    private ProjectAssignment() { }

    public static ProjectAssignment Create(
        Guid projectId,
        Guid employeeId,
        string? roleOnProject,
        int allocationPercentage,
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (allocationPercentage < 0 || allocationPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(allocationPercentage), "Allocation percentage must be between 0 and 100.");

        if (endDate.HasValue && endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        return new ProjectAssignment
        {
            ProjectId = projectId,
            EmployeeId = employeeId,
            RoleOnProject = roleOnProject,
            AllocationPercentage = allocationPercentage,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateAllocation(int? allocationPercentage, string? roleOnProject, DateOnly? endDate)
    {
        if (allocationPercentage.HasValue)
        {
            if (allocationPercentage.Value < 0 || allocationPercentage.Value > 100)
                throw new ArgumentOutOfRangeException(nameof(allocationPercentage), "Allocation percentage must be between 0 and 100.");
            AllocationPercentage = allocationPercentage.Value;
        }

        if (roleOnProject is not null)
            RoleOnProject = roleOnProject;

        if (endDate.HasValue)
        {
            if (endDate <= StartDate)
                throw new ArgumentException("End date must be after start date.");
            EndDate = endDate;
        }

        MarkUpdated();
    }

    public void EndAssignment()
    {
        IsActive = false;
        EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        MarkUpdated();
    }
}
