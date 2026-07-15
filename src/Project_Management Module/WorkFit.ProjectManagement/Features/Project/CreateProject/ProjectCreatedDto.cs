namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed record ProjectCreatedDto(
    Guid Id,
    string Name,
    string Status,
    Guid TeamLeaderId,
    DateTimeOffset CreatedAt);

public sealed record RequiredSkillInputDto(
    Guid SkillId,
    string Level,
    int Priority);
