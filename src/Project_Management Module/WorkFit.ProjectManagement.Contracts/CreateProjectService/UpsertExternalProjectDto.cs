namespace WorkFit.ProjectManagement.Contracts.CreateProjectService;

public sealed record UpsertExternalProjectDto(
    Guid OrganizationId,
    string SourceSystem,
    string SourceReferenceId,
    string Name,
    string? Description
);
