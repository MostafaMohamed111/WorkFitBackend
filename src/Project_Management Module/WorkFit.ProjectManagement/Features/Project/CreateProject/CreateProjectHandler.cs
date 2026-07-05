using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ProjectCreatedDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public CreateProjectHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<ProjectCreatedDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? ProjectStatus.Planning
            : request.Status.ToProjectStatus();

        var project = Domain.Entities.Project.Create(
            request.DepartmentId,
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            status);

        if (request.RequiredSkills is { Count: > 0 })
        {
            var requiredSkills = request.RequiredSkills
                .Select(s => ProjectRequiredSkill.Create(project.Id, s.SkillId, s.Level.ToSkillLevel(), s.Priority))
                .ToList();

            project.ReplaceRequiredSkills(requiredSkills);
        }

        await _projectRepository.AddAsync(project, cancellationToken);

        var log = ProjectActivityLog.Create(
            project.Id,
            _currentUser.GetUserId(cancellationToken),
            ActivityActions.ProjectCreated,
            ActivityEntityType.Project,
            project.Id);

        await _projectRepository.AddActivityLogAsync(log, cancellationToken);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return new ProjectCreatedDto(project.Id, project.Name, project.Status.ToApiString(), project.CreatedAt);
    }
}
