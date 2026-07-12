namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record ConfidenceEvidenceAiDto(
    Guid Id,
    string Source,
    string Details,
    DateTime EvidenceDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
