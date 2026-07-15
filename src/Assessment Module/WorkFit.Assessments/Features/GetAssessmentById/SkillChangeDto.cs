namespace WorkFit.Assessments.Features.GetAssessmentById;

internal sealed record SkillChangeDto(
        Guid skillId,
        string skillName,
        int OldScore,
        int ProposedScore,
        string Evidence
    );