namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

public sealed record ProjectUpdatedDto(
    Guid Id,
    string Name,
    string Status);
