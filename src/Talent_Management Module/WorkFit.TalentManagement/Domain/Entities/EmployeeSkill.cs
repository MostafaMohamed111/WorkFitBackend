using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class EmployeeSkill : BaseEntity
{
    public Guid EmployeeProfileId { get; private set; }
    public Guid SkillId { get; private set; } // ref to skills module
    public string SkillName { get; private set; } = default!;
    public int ConfidenceScore { get; private set; } // cached score from SkillConfidenceChange
    public EmployeeProfile Employee { get; private set; } = default!;
    private readonly List<SkillConfidenceChange> _confidenceChanges = new List<SkillConfidenceChange>();
    public IReadOnlyCollection<SkillConfidenceChange> ConfidenceChanges => _confidenceChanges;

    public static EmployeeSkill Create(Guid empId, Guid skillId, Guid assessmentId,
        string skillName, int confidenceScore, string details, string source )
    {
        // validation here
        var skill = new EmployeeSkill()
        {
            EmployeeProfileId = empId,
            SkillId = skillId,
            SkillName = skillName,
        };

        skill.ApplyAssessedChange(assessmentId, confidenceScore, details, source);
        return skill;
    }

    public void ApplyAssessedChange(Guid assessmentId, int newConfidenceScore, string details, string source)
    {
        var confidenceChange = SkillConfidenceChange.Create(Id, assessmentId, 0, newConfidenceScore);
        confidenceChange.AddEvidence(details, source);
        _confidenceChanges.Add(confidenceChange);
        ConfidenceScore = newConfidenceScore;
    }

}