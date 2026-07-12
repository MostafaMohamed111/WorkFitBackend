namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record SkillConfidenceChangeAiDto(
    Guid Id,
    Guid AssessmentId,
    int OldScore,
    int NewScore,
    DateTime CreatedAt,
    IReadOnlyList<ConfidenceEvidenceAiDto> ConfidenceEvidences);
