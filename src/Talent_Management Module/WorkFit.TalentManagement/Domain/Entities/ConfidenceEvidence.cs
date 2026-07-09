using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class ConfidenceEvidence : BaseEntity
{ 
    public Guid SkillConfidenceChangeId { get; private set; }
    public string Source { get; private set; } = default!; // GitHub / Jira / Manual
    public string Details { get; private set; } = default!; // commit URL, ticket link...
    public DateTime EvidenceDate { get; private set; }



    public static ConfidenceEvidence Create(Guid skillConfidenceChangeId,
        string source, string details)
    {
        // validation here
        return new ConfidenceEvidence()
        {
            SkillConfidenceChangeId = skillConfidenceChangeId,
            Source = source,
            Details = details,
            EvidenceDate = DateTime.UtcNow
        };
    }
}
