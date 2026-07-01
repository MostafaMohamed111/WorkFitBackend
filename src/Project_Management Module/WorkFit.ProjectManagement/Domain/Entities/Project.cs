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

    public ICollection<ProjectRequiredSkill> RequiredSkills { get; private set; }

    public ICollection<ProjectTask> Tasks { get; private set; }

    public ICollection<ProjectAssignment> Assignments { get; private set; }

    public ICollection<ProjectDomain> Domains { get; private set; }

    public ICollection<ProjectActivityLog> ActivityLogs { get; private set; }
}
