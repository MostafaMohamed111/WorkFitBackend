namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record EmployeeDetailsDto(
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
    IReadOnlyList<EmployeeSkillLookUpDto> Skills,
    IReadOnlyList<EmployeeIdentityMappingDetailsDto> IdentityMappings,
    IReadOnlyList<EmployeeCertificationDetailsDto> Certifications);
