
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.RemoveProjectDomain;

public sealed record RemoveProjectDomainCommand(Guid ProjectId, Guid DomainId) : IRequest<DomainDeletedDto?>;
