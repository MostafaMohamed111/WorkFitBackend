
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

public sealed class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ProjectUpdatedDto?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public UpdateProjectHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectUpdatedDto?> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return null;

        var beforeState = System.Text.Json.JsonSerializer.Serialize(new
        {
            project.Name,
            project.Description,
            Status = project.Status.ToApiString(),
            project.EndDate
        });

        project.UpdateDetails(request.Name, request.Description, request.EndDate);

        if (!string.IsNullOrWhiteSpace(request.Status))
            project.ChangeStatus(request.Status.ToProjectStatus());

        if (request.RequiredSkills is not null)
        {
            var requiredSkills = request.RequiredSkills
                .Select(s => ProjectRequiredSkill.Create(project.Id, s.SkillId, s.Level.ToSkillLevel(), s.Priority))
                .ToList();

            project.ReplaceRequiredSkills(requiredSkills);
        }

        await _projectRepository.UpdateAsync(project, cancellationToken);

        var afterState = System.Text.Json.JsonSerializer.Serialize(new
        {
            project.Name,
            project.Description,
            Status = project.Status.ToApiString(),
            project.EndDate
        });

        var log = ProjectActivityLog.Create(
            project.Id,
            _currentUser.GetUserId(cancellationToken),
            ActivityActions.ProjectUpdated,
            ActivityEntityType.Project,
            project.Id,
            beforeState,
            afterState);

        project.ActivityLogs.Add(log);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectUpdatedDto(project.Id, project.Name, project.Status.ToApiString());
    }
}
