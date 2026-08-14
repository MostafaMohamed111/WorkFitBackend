
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
            var requestedSkills = request.RequiredSkills
                .GroupBy(s => s.SkillId)
                .ToDictionary(group => group.Key, group => group.Last());

            foreach (var existingSkill in project.RequiredSkills.ToList())
            {
                if (requestedSkills.Remove(existingSkill.SkillId, out var requestedSkill))
                {
                    existingSkill.Update(requestedSkill.Level.ToSkillLevel(), requestedSkill.Priority);
                }
                else
                {
                    project.RequiredSkills.Remove(existingSkill);
                }
            }

            foreach (var requestedSkill in requestedSkills.Values)
            {
                project.RequiredSkills.Add(ProjectRequiredSkill.Create(
                    project.Id,
                    requestedSkill.SkillId,
                    requestedSkill.Level.ToSkillLevel(),
                    requestedSkill.Priority));
            }
        }

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectUpdatedDto(project.Id, project.Name, project.Status.ToApiString());
    }
}
