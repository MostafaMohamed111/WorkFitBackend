namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record EmployeeIdentityMappingDetailsDto(
    Guid Id,
    string SourceSystem,
    string ExternalAccountId,
    string ExternalDisplayName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
