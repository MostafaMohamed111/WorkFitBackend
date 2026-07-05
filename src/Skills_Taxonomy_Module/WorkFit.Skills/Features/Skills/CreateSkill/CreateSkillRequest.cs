using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed record CreateSkillRequest(
    string Name,
    string? Description,
    SkillOrigin Origin,
    Guid? CategoryId,
    Guid? GroupId,
    Guid? ParentSkillId,
    int EstimatedTimeToLearn
);