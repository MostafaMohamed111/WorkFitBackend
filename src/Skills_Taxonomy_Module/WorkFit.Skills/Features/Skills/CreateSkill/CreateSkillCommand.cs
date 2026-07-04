using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed record CreateSkillCommand(
    string Name,
    string? Description,
    SkillOrigin Origin,
    Guid? OrganizationId,
    Guid? CategoryId,
    Guid? GroupId,
    Guid? ParentSkillId,
    int EstimatedTimeToLearn
) : IRequest<SkillDto>;