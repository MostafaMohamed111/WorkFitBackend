using WorkFit.SharedKernel.BaseEntity;
using WorkFit.TalentManagement.Domain.Enums;

namespace WorkFit.TalentManagement.Domain.Entities;

public class EmployeeSkill : BaseEntity
{ 
    public Guid EmployeeId { get; private set; }
    public Guid SkillId { get; private set; } // reference to Module 4

    // Cached from Module 4 عشان منحتاجش cross-module join
    public string SkillName { get; private set; } = default!;
    public ProficiencyLevel Proficiency { get; private set; }
    public string Source { get; private set; } = "Manual"; // Manual / GitHub / Jira
    public Employee Employee { get; private set; } = default!;
    public ICollection<SkillEvidence> Evidences { get; private set; } = [];

    public static EmployeeSkill Create(Guid empId, Guid skillId,
        string skillName, ProficiencyLevel proficiency) => new()
        {
            EmployeeId = empId,
            SkillId = skillId,
            SkillName = skillName,
            Proficiency = proficiency
        };

    public void UpdateProficiency(ProficiencyLevel level)
        => Proficiency = level;
}