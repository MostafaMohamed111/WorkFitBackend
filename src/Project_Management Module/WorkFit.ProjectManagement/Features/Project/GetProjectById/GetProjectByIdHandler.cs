using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

public sealed class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto?>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDetailDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        // org_id scoping is applied inside the repository via the ORM tenancy middleware,
        // so no explicit org filter is needed here.
        return await _projectRepository.GetDetailByIdAsync(request.Id, cancellationToken);
    }
}
