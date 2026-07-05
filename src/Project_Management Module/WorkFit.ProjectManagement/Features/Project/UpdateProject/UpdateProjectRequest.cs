namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

/// <summary>
/// Maps to PUT /api/projects/{id}. Partial update — only supplied fields are changed.
/// </summary>
public sealed class UpdateProjectRequest
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>When supplied, replaces the full required_skills array.</summary>
    public List<CreateProject.RequiredSkillInputDto>? RequiredSkills { get; set; }
}
