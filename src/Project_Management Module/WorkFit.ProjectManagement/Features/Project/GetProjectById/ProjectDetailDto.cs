using WorkFit.ProjectManagement.Domain.Enums;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

public sealed record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid DepartmentId,
    string Status,
    Guid? TeamLeaderId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<RequiredSkillDto> RequiredSkills,
    SourceSystem? sourceSystem,
    string? SourceReferenceId,
    DateTimeOffset CreatedAt,
    double CoveragePct);

public sealed record RequiredSkillDto(
    Guid SkillId,
    string SkillName,
    string Level,
    int Priority);
