namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

public sealed record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid DepartmentId,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<RequiredSkillDto> RequiredSkills,
    string? SourceSystem,
    string? SourceReferenceId,
    DateTimeOffset CreatedAt,
    double CoveragePct);

public sealed record RequiredSkillDto(
    Guid SkillId,
    string SkillName,
    string Level,
    int Priority);
