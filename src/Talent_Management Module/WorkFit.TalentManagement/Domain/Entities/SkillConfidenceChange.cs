

using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class SkillConfidenceChange : BaseEntity
{
    public Guid EmployeeSkillId { get; private set; }
    public Guid AssessmentId { get; private set; }
    public int OldScore { get; private set; }
    public int NewScore { get; private set; }
    private readonly List<ConfidenceEvidence> _confidenceEvidences = new List<ConfidenceEvidence>();
    public IReadOnlyCollection<ConfidenceEvidence> ConfidenceEvidences => _confidenceEvidences;

    private SkillConfidenceChange() // EF
    {
        
    }

    private SkillConfidenceChange(Guid skillId,
        Guid assessmentId,
        int oldScore,
        int newScore
        )
    {
        EmployeeSkillId = skillId;
        AssessmentId = assessmentId;
        OldScore = oldScore;
        NewScore = newScore;
    }

    public static SkillConfidenceChange Create(Guid skillId,
        Guid assessmentId,
        int oldScore,
        int newScore)
    {
        // validation here
        var skillConfidenceChange = new SkillConfidenceChange(skillId, assessmentId, oldScore, newScore);
        return skillConfidenceChange;
    }

    public void AddEvidence(string details, string source)
    {
        var evidence = ConfidenceEvidence.Create(Id, source, details);
        _confidenceEvidences.Add(evidence);
    }
}
