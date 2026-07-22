using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

public sealed class UpdateProjectStatusHandler : IRequestHandler<UpdateProjectStatusCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public UpdateProjectStatusHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UpdateProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project is null)
            throw new EntityNotFoundException(
                    ModuleMarker.ModuleName,
                    typeof(Domain.Entities.Project).Name,
                    command.Id
                );

        var actorId = _currentUser.GetUserId(cancellationToken);
        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        // Project.ChangeStatus enforces the allowed transition graph and throws
        // InvalidOperationException on an illegal transition (e.g. completed -> active).
        project.ChangeStatus(actorId, command.Status.ToProjectStatus());

        // project.status_changed also notifies all active project members (handled
        // by the notification service subscribed to project_activity_logs inserts).

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
