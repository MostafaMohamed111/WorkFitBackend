using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.RemoveProjectDomain;

public sealed class RemoveProjectDomainHandler : IRequestHandler<RemoveProjectDomainCommand, DomainDeletedDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public RemoveProjectDomainHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<DomainDeletedDto?> Handle(RemoveProjectDomainCommand request, CancellationToken cancellationToken)
    {
        if (!await _projectRepository.ExistsAsync(request.ProjectId, cancellationToken))
            return null;

        var removed = await _projectRepository.RemoveDomainAsync(request.ProjectId, request.DomainId, cancellationToken);
        if (!removed)
            return new DomainDeletedDto(false);

        var log = ProjectActivityLog.Create(
            request.ProjectId,
            _currentUser.GetUserId(cancellationToken),
            ActivityActions.ProjectDomainRemoved,
            ActivityEntityType.Domain,
            request.DomainId);

        await _projectRepository.AddActivityLogAsync(log, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new DomainDeletedDto(true);
    }
}
