using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Infrastructure.Data.Repositories;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserContext _currentUser;

    public CreateProjectCommandHandler(IProjectRepository projectRepository, ICurrentUserContext currentUser)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(command.Status)
            ? ProjectStatus.Planning
            : command.Status.ToProjectStatus();

        var project = Domain.Entities.Project.Create(
            command.OrganizationId,
            command.Name,
            command.AttachedDocumentIds,
            command.Description,
            command.StartDate,
            command.EndDate,
            _currentUser.GetUserId(cancellationToken),
            status);

        if (command.RequiredSkills is { Count: > 0 })
        {
            var requiredSkills = command.RequiredSkills
                .Select(s => ProjectRequiredSkill.Create(project.Id, s.SkillId, s.Level.ToSkillLevel(), s.Priority))
                .ToList();

            project.ReplaceRequiredSkills(requiredSkills);
        }

        await _projectRepository.AddAsync(project, cancellationToken);

        await _projectRepository.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
