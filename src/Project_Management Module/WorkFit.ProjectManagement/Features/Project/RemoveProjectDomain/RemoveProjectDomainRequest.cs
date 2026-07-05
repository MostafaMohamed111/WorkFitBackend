namespace WorkFit.ProjectManagement.Features.Project.RemoveProjectDomain;

/// <summary>
/// Maps to DELETE /api/projects/{id}/domains/{domainId}.
/// </summary>
public sealed class RemoveProjectDomainRequest
{
    public Guid Id { get; set; }

    public Guid DomainId { get; set; }
}
