namespace WorkFit.TalentManagement.CrossCutting.Dtos;

public sealed record EmployeeIdentityMappingAiDto(
    Guid Id,
    string SourceSystem,
    string ExternalAccountId,
    string ExternalDisplayName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
