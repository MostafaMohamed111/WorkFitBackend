namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

/// <summary>
/// Maps to PUT /api/projects/{id}/status. Validates state-machine transitions
/// (e.g. completed/cancelled are terminal).
/// </summary>
public sealed record UpdateProjectStatusRequest(Guid Id, string Status);