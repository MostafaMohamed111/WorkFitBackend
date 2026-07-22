
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

public sealed class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectUpdatedDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public UpdateProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectUpdatedDto?> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return null;

        var actorId = _currentUser.GetUserId(cancellationToken);
        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        project.UpdateDetails(request.Name, request.Description, request.EndDate);

        if (request.RequiredSkills is not null)
        {
            var requiredSkills = request.RequiredSkills
                .Select(s => ProjectRequiredSkill.Create(project.Id, s.SkillId, s.Level.ToSkillLevel(), s.Priority))
                .ToList();

            project.ReplaceRequiredSkills(requiredSkills);
        }

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectUpdatedDto(project.Id, project.Name, project.Status.ToApiString());
    }
}
