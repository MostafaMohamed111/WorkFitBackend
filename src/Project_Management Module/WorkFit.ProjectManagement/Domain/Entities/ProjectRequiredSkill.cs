
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
}

