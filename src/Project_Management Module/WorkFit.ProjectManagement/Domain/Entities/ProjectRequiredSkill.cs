using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class ProjectRequiredSkill : BaseEntity
{
    public Guid ProjectId { get; private set; }

    public Guid SkillId { get; private set; }

    public SkillLevel Level { get; private set; }

    public int Priority { get; private set; }

    public Project Project { get; private set; } = default!;

    private ProjectRequiredSkill() { }

    public static ProjectRequiredSkill Create(Guid projectId, Guid skillId, SkillLevel level, int priority)
    {
        if (priority is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 1 and 5.");

        return new ProjectRequiredSkill
        {
            ProjectId = projectId,
            SkillId = skillId,
            Level = level,
            Priority = priority
        };
    }
}
