using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.AddProjectDomain;

public sealed record AddProjectDomainCommand(Guid ProjectId, Guid DomainId) : IRequest<ProjectDomainTaggedDto?>;
