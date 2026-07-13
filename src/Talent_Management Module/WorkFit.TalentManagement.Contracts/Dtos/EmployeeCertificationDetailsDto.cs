namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record EmployeeCertificationDetailsDto(
    Guid Id,
    Guid DocumentId,
    string Name,
    string IssuingOrganization,
    DateOnly IssueDate,
    DateOnly? ExpiryDate,
    string Details,
    bool IsExpired,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
