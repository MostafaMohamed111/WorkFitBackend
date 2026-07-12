namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record EmployeeCertificationAiDto(
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
