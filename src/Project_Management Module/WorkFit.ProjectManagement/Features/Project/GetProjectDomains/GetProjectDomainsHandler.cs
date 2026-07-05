
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectDomains;

public sealed class GetProjectDomainsHandler : IRequestHandler<GetProjectDomainsQuery, ProjectDomainListDto?>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectDomainsHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDomainListDto?> Handle(GetProjectDomainsQuery request, CancellationToken cancellationToken)
    {
        if (!await _projectRepository.ExistsAsync(request.ProjectId, cancellationToken))
            return null;

        var domains = await _projectRepository.GetDomainsAsync(request.ProjectId, cancellationToken);
        return new ProjectDomainListDto(domains);
    }
}
