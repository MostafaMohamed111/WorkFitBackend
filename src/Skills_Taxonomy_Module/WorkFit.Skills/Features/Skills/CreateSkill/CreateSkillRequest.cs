using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed record CreateSkillRequest
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public SkillOrigin Origin { get; init; } = SkillOrigin.Custom;
    public Guid? CategoryId { get; init; }
    public Guid? GroupId { get; init; }
    public Guid? ParentSkillId { get; init; }
    public int EstimatedTimeToLearn { get; init; } = 40;
}