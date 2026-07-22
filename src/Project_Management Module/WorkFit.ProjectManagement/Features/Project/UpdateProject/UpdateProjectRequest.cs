namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

/// <summary>
/// Maps to PUT /api/projects/{id}. Partial update — only supplied fields are changed.
/// </summary>
public sealed record UpdateProjectRequest(Guid Id, string? Name, string? Description, DateOnly? EndDate, List<CreateProject.RequiredSkillInputDto>? RequiredSkills);