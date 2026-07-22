

using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public ArchiveProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName,
                typeof(Domain.Entities.Project).Name,
                request.Id);

        var actorId = _currentUser.GetUserId(cancellationToken);

        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        // Archiving cascades: assignments are soft-ended (is_active = FALSE) by the
        // repository/infrastructure layer; tasks are retained untouched per spec.
        project.Archive(actorId);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
