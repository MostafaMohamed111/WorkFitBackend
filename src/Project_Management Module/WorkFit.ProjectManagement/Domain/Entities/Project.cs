using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class Project : BaseEntity
{
    public Guid DepartmentId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public string? SourceSystem { get; private set; }

    public string? SourceReferenceId { get; private set; }

    public ICollection<ProjectRequiredSkill> RequiredSkills { get; private set; } = new List<ProjectRequiredSkill>();

    public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();

    public ICollection<ProjectAssignment> Assignments { get; private set; } = new List<ProjectAssignment>();

    public ICollection<ProjectDomain> Domains { get; private set; } = new List<ProjectDomain>();

    public ICollection<ProjectActivityLog> ActivityLogs { get; private set; } = new List<ProjectActivityLog>();

    private Project() { }

    public static Project Create(
        Guid departmentId,
        string name,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        ProjectStatus status = ProjectStatus.Planning,
        string? sourceSystem = null,
        string? sourceReferenceId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
            throw new ArgumentException("Project name must be between 3 and 100 characters.", nameof(name));

        if (description is not null && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));

        if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        return new Project
        {
            DepartmentId = departmentId,
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            SourceSystem = sourceSystem,
            SourceReferenceId = sourceReferenceId
        };
    }

    public void UpdateDetails(string? name, string? description, DateOnly? endDate)
    {
        if (name is not null)
        {
            if (name.Length < 3 || name.Length > 100)
                throw new ArgumentException("Project name must be between 3 and 100 characters.", nameof(name));
            Name = name;
        }

        if (description is not null)
        {
            if (description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
            Description = description;
        }

        if (endDate.HasValue)
        {
            if (StartDate.HasValue && endDate <= StartDate)
                throw new ArgumentException("End date must be after start date.");
            EndDate = endDate;
        }

        MarkUpdated();
    }

    public void ReplaceRequiredSkills(ICollection<ProjectRequiredSkill> requiredSkills)
    {
        RequiredSkills = requiredSkills;
        MarkUpdated();
    }

    public void ChangeStatus(ProjectStatus newStatus)
    {
        if (!Status.CanTransitionToInternal(newStatus))
            throw new InvalidOperationException($"Cannot transition project from '{Status}' to '{newStatus}'.");

        Status = newStatus;
        MarkUpdated();
    }

    /// <summary>
    /// Soft-archives the project (DELETE /api/projects/{id}). Hard delete is never performed.
    /// </summary>
    public void Archive()
    {
        Status = ProjectStatus.Cancelled;
        MarkUpdated();
    }
}

file static class ProjectStatusInternalExtensions
{
    // Mirrors WorkFit.ProjectManagement.Features.Common.EnumMappingExtensions.CanTransitionTo,
    // kept internal to the domain layer so Project stays free of a Features-layer dependency.
    public static bool CanTransitionToInternal(this ProjectStatus current, ProjectStatus target)
    {
        if (current == target) return false;

        return current switch
        {
            ProjectStatus.Planning => target is ProjectStatus.Active or ProjectStatus.Cancelled,
            ProjectStatus.Active => target is ProjectStatus.OnHold or ProjectStatus.Completed or ProjectStatus.Cancelled,
            ProjectStatus.OnHold => target is ProjectStatus.Active or ProjectStatus.Cancelled,
            ProjectStatus.Completed => false,
            ProjectStatus.Cancelled => false,
            _ => false
        };
    }
}
