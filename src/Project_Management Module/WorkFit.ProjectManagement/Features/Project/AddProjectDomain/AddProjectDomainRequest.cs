namespace WorkFit.ProjectManagement.Features.Project.AddProjectDomain;

/// <summary>
/// Maps to POST /api/projects/{id}/domains.
/// </summary>
public sealed class AddProjectDomainRequest
{
    public Guid Id { get; set; }

    public Guid DomainId { get; set; }
}
