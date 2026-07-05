using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

public sealed class UpdateProjectStatusHandler : IRequestHandler<UpdateProjectStatusCommand, ProjectStatusDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public UpdateProjectStatusHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectStatusDto?> Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return null;

        var beforeStatus = project.Status.ToApiString();

        // Project.ChangeStatus enforces the allowed transition graph and throws
        // InvalidOperationException on an illegal transition (e.g. completed -> active).
        project.ChangeStatus(request.Status.ToProjectStatus());

        await _projectRepository.UpdateAsync(project, cancellationToken);

        var log = ProjectActivityLog.Create(
            project.Id,
            _currentUser.GetUserId(cancellationToken),
            ActivityActions.ProjectStatusChanged,
            ActivityEntityType.Project,
            project.Id,
            beforeState: $"{{\"status\":\"{beforeStatus}\"}}",
            afterState: $"{{\"status\":\"{project.Status.ToApiString()}\"}}");

        project.ActivityLogs.Add(log);

        // project.status_changed also notifies all active project members (handled
        // by the notification service subscribed to project_activity_logs inserts).

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectStatusDto(project.Id, project.Status.ToApiString());
    }
}
