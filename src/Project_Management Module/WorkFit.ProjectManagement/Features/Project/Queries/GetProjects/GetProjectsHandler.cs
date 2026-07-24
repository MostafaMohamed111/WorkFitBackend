using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjects;

public sealed class GetProjectsHandler
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectListItemDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        return await _projectRepository.GetProjectsAsync(
            request.Status,
            request.OrganizationId,
            request.Page,
            request.Limit,
            cancellationToken);
    }
}