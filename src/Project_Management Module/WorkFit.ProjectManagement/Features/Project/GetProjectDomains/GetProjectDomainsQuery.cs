using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectDomains;

public sealed record GetProjectDomainsQuery(Guid ProjectId) : IRequest<ProjectDomainListDto?>;
