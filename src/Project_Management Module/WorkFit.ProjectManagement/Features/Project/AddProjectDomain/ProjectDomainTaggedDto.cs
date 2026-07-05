namespace WorkFit.ProjectManagement.Features.Project.AddProjectDomain;

public sealed record ProjectDomainTaggedDto(
    Guid ProjectId,
    Guid DomainId);
