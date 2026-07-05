namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

/// <summary>
/// Maps to POST /api/projects. org_id is injected server-side from the JWT — never accepted here.
/// </summary>
public sealed class CreateProjectRequest
{
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    /// <summary>Optional. Defaults to "planning" server-side when omitted.</summary>
    public string? Status { get; set; }

    public List<RequiredSkillInputDto>? RequiredSkills { get; set; }
}
