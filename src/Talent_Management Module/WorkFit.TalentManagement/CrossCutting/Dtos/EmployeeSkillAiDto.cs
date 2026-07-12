namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record EmployeeSkillAiDto(
    Guid Id,
    Guid SkillId,
    string SkillName,
    int ConfidenceScore,
    IReadOnlyList<SkillConfidenceChangeAiDto> ConfidenceChanges);
