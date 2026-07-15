namespace WorkFit.Assessments.Features.Queries.Dtos;

internal sealed record SkillChangeDto(
        Guid skillId,
        string skillName,
        int OldScore,
        int ProposedScore,
        string Evidence
    );