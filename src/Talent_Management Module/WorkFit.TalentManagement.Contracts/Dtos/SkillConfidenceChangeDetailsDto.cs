namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record SkillConfidenceChangeDetailsDto(
    Guid Id,
    Guid? AssessmentId,
    int OldScore,
    int NewScore,
    DateTime CreatedAt,
    IReadOnlyList<ConfidenceEvidenceDetailsDto> ConfidenceEvidences);
