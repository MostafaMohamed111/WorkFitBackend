using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Features.Skills;

public sealed record SkillDto(
    Guid Id,
    string Name,
    string NormalizedName,
    string? Description,
    SkillOrigin Origin,
    Guid? OrganizationId,
    Guid? CategoryId,
    Guid? GroupId,
    Guid? ParentSkillId,
    bool IsDeleted,
    int EstimatedTimeToLearn,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);