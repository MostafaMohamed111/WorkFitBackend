namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

/// <summary>
/// Maps to GET /api/projects/{id}. The route id is bound to Id by FastEndpoints.
/// </summary>
public sealed class GetProjectByIdRequest
{
    public Guid Id { get; set; }
}
