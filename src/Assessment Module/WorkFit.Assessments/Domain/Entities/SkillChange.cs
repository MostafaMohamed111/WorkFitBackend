using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Assessments.Domain.Entities;

internal sealed class SkillChange : BaseEntity
{
    public Guid AssessmentId { get; private set; }
    public Guid SkillId { get; private set; }
    public string SkillName { get; private set; } = default!;
    public int OldScore { get; private set; }
    public int ProposedScore { get; private set; }
    public int NewScore { get; private set; }
    public bool ScoreUpdated { get; private set; } 
    public string EvidenceDescription { get; private set; } = default!;
    public string? ProcessorNote { get; private set; }


    private SkillChange() { } // EF

    private SkillChange(Guid assessmentId, Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDescription)
    {
        AssessmentId = assessmentId;
        SkillId = skillId;
        SkillName = skillName;
        OldScore = oldScore;
        ProposedScore = proposedScore;
        NewScore = ProposedScore; // unless updated by team lead
        EvidenceDescription = evidenceDescription;
        ScoreUpdated = false;
    }

    public static SkillChange Create(Guid assessmentId, Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDescription)
    {
        // validation 
        return new SkillChange(assessmentId, skillId, skillName, oldScore, proposedScore, evidenceDescription);
    }

    public void UpdateScore(int newScore, string? processorNote = null)
    {
        // validation 
        NewScore = newScore;
        ScoreUpdated = true;
        ProcessorNote = processorNote;
    }

}