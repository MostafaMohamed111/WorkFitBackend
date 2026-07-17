using System.Text.Json;
using WorkFit.Recommendations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Recommendations.Domain.Entities;

public sealed class Recommendation : BaseEntity
{
    public Guid TaskId { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public string RequiredSkillsSnapshot { get; private set; } = default!;

    private readonly List<RecommendationCandidate> _candidates = new();
    public IReadOnlyCollection<RecommendationCandidate> Candidates => _candidates;

    private Recommendation() { }

    public static Recommendation Create(
        Guid taskId,
        IReadOnlyList<Guid> requiredSkillIds,
        IEnumerable<(Guid EmployeeId, decimal MatchScore, string MatchReasoning)> candidateInputs)
    {
        var rec = new Recommendation
        {
            TaskId = taskId,
            GeneratedAt = DateTimeOffset.UtcNow,
            RequiredSkillsSnapshot = JsonSerializer.Serialize(requiredSkillIds)
        };

        var ranked = candidateInputs
            .OrderByDescending(c => c.MatchScore)
            .Select((c, i) => RecommendationCandidate.Create(
                rec.Id, c.EmployeeId, c.MatchScore, c.MatchReasoning, rank: i + 1))
            .ToList();

        rec._candidates.AddRange(ranked);
        return rec;
    }

    public void ApproveCandidate(Guid employeeId, Guid actionedBy)
    {
        var targetCandidate = _candidates.FirstOrDefault(c => c.EmployeeId == employeeId)
            ?? throw new CandidateNotPartOfRecommendationException(Id, employeeId);

        // If it's already approved, do nothing
        if (targetCandidate.Status == Enums.CandidateStatus.Approved)
        {
            return;
        }

        // Mark target as approved
        targetCandidate.MarkAsApproved(actionedBy);

        // Mark all other non-rejected candidates as rejected
        var otherCandidates = _candidates.Where(c => c.EmployeeId != employeeId && c.Status != Enums.CandidateStatus.Rejected);
        foreach (var candidate in otherCandidates)
        {
            candidate.MarkAsRejected(actionedBy);
        }
    }

    public void RejectCandidate(Guid employeeId, Guid actionedBy)
    {
        var targetCandidate = _candidates.FirstOrDefault(c => c.EmployeeId == employeeId)
            ?? throw new CandidateNotPartOfRecommendationException(Id, employeeId);

        // If it's already rejected, do nothing
        if (targetCandidate.Status == Enums.CandidateStatus.Rejected)
        {
            return;
        }

        targetCandidate.MarkAsRejected(actionedBy);
    }
}
