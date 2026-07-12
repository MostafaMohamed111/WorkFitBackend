namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record EmployeeAiDto(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    string Name,
    string? Email,
    string? Bio,
    string? LinkedInUrl,
    string JobTitle,
    string Status,
    bool IsActive,
    int CurrentAllocationPercentage,
    DateOnly? HireDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<EmployeeSkillAiDto> Skills,
    IReadOnlyList<EmployeeIdentityMappingAiDto> IdentityMappings,
    IReadOnlyList<EmployeeCertificationAiDto> Certifications);
