namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

/// <summary>
/// Maps to DELETE /api/projects/{id}. This is a soft-archive (status -> cancelled);
/// hard delete is blocked per the Module 5 spec.
/// </summary>
public sealed class ArchiveProjectRequest
{
    public Guid Id { get; set; }
}
