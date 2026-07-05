
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class ArchiveProjectHandler : IRequestHandler<ArchiveProjectCommand, ProjectArchivedDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public ArchiveProjectHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectArchivedDto?> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return null;

        // Archiving cascades: assignments are soft-ended (is_active = FALSE) by the
        // repository/infrastructure layer; tasks are retained untouched per spec.
        project.Archive();

        await _projectRepository.ArchiveAsync(project.Id, cancellationToken);

        var log = ProjectActivityLog.Create(
            project.Id,
            _currentUser.GetUserId(cancellationToken),
            ActivityActions.ProjectArchived,
            ActivityEntityType.Project,
            project.Id);

        project.ActivityLogs.Add(log);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectArchivedDto(project.Id, project.Status.ToApiString());
    }
}
