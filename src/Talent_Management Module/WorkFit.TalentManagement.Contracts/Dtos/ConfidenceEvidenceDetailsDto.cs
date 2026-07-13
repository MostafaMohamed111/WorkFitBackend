namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record ConfidenceEvidenceDetailsDto(
    Guid Id,
    string Source,
    string Details,
    DateTime EvidenceDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
