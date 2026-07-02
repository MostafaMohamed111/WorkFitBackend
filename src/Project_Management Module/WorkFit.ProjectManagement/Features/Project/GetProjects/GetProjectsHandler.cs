using WorkFit.ProjectManagement.Features.Project.GetProjects;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Projects.GetProjects;

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
            request.DepartmentId,
            request.Page,
            request.Limit,
            cancellationToken);
    }
}