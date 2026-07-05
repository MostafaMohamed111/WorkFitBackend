namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

/// <summary>
/// Maps to PUT /api/projects/{id}/status. Validates state-machine transitions
/// (e.g. completed/cancelled are terminal).
/// </summary>
public sealed class UpdateProjectStatusRequest
{
    public Guid Id { get; set; }

    public string Status { get; set; } = default!;
}
