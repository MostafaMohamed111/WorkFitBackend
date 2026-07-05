namespace WorkFit.ProjectManagement.Features.Project.GetProjectDomains;

/// <summary>
/// Maps to GET /api/projects/{id}/domains.
/// </summary>
public sealed class GetProjectDomainsRequest
{
    public Guid Id { get; set; }
}
