
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed record CreateProjectCommand(
    string Name,
    string? Description,
    Guid DepartmentId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Status,
    List<RequiredSkillInputDto>? RequiredSkills) : IRequest<ProjectCreatedDto>;
