namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record EmployeeSkillLookUpDto(
    Guid Id,
    Guid SkillId,
    string SkillName,
    int ConfidenceScore,
    IReadOnlyList<SkillConfidenceChangeDetailsDto> ConfidenceChanges);
