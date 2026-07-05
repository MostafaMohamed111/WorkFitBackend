using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

/// <summary>
/// Junction entity tagging a Project with a business domain (Fintech, Healthcare, etc.).
/// Composite key (ProjectId, DomainId) — no surrogate Id, per project_domains schema.
/// </summary>
public class ProjectDomain
{
    public Guid ProjectId { get; private set; }

    public Guid DomainId { get; private set; }

    public Project Project { get; private set; } = default!;

    private ProjectDomain() { }

    public static ProjectDomain Create(Guid projectId, Guid domainId)
    {
        return new ProjectDomain
        {
            ProjectId = projectId,
            DomainId = domainId
        };
    }
}
