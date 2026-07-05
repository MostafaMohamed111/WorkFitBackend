namespace WorkFit.ProjectManagement.Features.Project.GetProjectDomains;

public sealed record ProjectDomainDto(
    Guid DomainId,
    string Name);

public sealed record ProjectDomainListDto(
    IReadOnlyList<ProjectDomainDto> Domains);
