namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed record SkillConfidenceChangeDto(
    Guid Id,
    Guid EmployeeSkillId,
    Guid AssessmentId,
    int OldScore,
    int NewScore,
    DateTime CreatedAt,
    IReadOnlyList<ConfidenceEvidenceDto> Evidences
);

public sealed record ConfidenceEvidenceDto(
    Guid Id,
    string Source,
    string Details,
    DateTime CreatedAt
);