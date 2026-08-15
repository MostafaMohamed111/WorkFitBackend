using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public sealed class ProjectMember : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public Guid EmployeeProfileId { get; private set; }
    public Project Project { get; private set; } = default!;

    private ProjectMember() { }

    public static ProjectMember Create(Guid projectId, Guid employeeProfileId)
    {
        if (projectId == Guid.Empty || employeeProfileId == Guid.Empty)
            throw new ArgumentException("Project and employee profile ids are required.");
        return new ProjectMember { ProjectId = projectId, EmployeeProfileId = employeeProfileId };
    }
}
