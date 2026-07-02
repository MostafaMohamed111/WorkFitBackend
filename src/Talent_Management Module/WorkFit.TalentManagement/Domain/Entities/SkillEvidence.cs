using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

public class SkillEvidence : BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EmployeeSkillId { get; private set; }
    public string Source { get; private set; } = default!; // GitHub / Jira / Manual
    public string Details { get; private set; } = default!; // commit URL, ticket link...
    public DateTime EvidenceDate { get; private set; } = DateTime.UtcNow;

    public EmployeeSkill EmployeeSkill { get; private set; } = default!;

    public static SkillEvidence Create(Guid empSkillId,
        string source, string details) => new()
        {
            EmployeeSkillId = empSkillId,
            Source = source,
            Details = details
        };
}
