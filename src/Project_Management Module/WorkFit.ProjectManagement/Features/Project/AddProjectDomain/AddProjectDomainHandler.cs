using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.AddProjectDomain;

public sealed class AddProjectDomainHandler : IRequestHandler<AddProjectDomainCommand, ProjectDomainTaggedDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public AddProjectDomainHandler(
        IProjectRepository projectRepository,
        ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectDomainTaggedDto?> Handle(
        AddProjectDomainCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _projectRepository.ExistsAsync(request.ProjectId, cancellationToken))
            return null;

        // Idempotent: if the domain is already tagged, return success.
        if (await _projectRepository.DomainTagExistsAsync(
                request.ProjectId,
                request.DomainId,
                cancellationToken))
        {
            return new ProjectDomainTaggedDto(
                request.ProjectId,
                request.DomainId);
        }

        var projectDomain = ProjectDomain.Create(
            request.ProjectId,
            request.DomainId);

        await _projectRepository.AddDomainAsync(
            projectDomain,
            cancellationToken);

        var log = ProjectActivityLog.Create(
            projectId: request.ProjectId,
            UserId: _currentUser.GetUserId(cancellationToken),
            action: ActivityActions.ProjectDomainAdded,
            entityType: ActivityEntityType.Domain,
            entityId: request.DomainId);

        await _projectRepository.AddActivityLogAsync(log, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectDomainTaggedDto(
            request.ProjectId,
            request.DomainId);
    }
}