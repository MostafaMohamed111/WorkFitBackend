
using WorkFit.ProjectManagement.Features.Project.CreateProject;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid Id,
    string? Name,
    string? Description,
    DateOnly? EndDate,
    List<RequiredSkillInputDto>? RequiredSkills) : IRequest<ProjectUpdatedDto?>;
