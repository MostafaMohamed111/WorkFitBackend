

using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectDomain 
{
    public Guid ProjectId { get; private set; }

    public Guid DomainId { get; private set; }

    public Project Project { get; private set; }
}
