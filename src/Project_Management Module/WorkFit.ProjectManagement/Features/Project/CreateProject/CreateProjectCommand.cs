
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.CreateProject;

public sealed record CreateProjectCommand(
    string Name,
    List<Guid> AttachedDocumentIds,
    string? Description,
    Guid OrganizationId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Status,
    List<RequiredSkillInputDto>? RequiredSkills) : IRequest<Guid>;
