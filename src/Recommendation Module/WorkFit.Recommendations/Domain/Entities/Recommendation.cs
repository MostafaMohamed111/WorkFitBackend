using System.Text.Json;
using WorkFit.Recommendations.Domain.Enums;
using WorkFit.Recommendations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Recommendations.Domain.Entities;

public sealed class Recommendation : BaseEntity
{
    public Guid TaskId { get; private set; }
    public string RequiredSkillsSnapshot { get; private set; } = default!;
    public Guid CreatedBy { get; private set; }

    private readonly List<RecommendationCandidate> _candidates = new();
    public IReadOnlyCollection<RecommendationCandidate> Candidates => _candidates;

    private Recommendation() { }

    public static Recommendation Create(
        Guid taskId,
        Guid createdById,
        IReadOnlyList<Guid> requiredSkillIds,
        IEnumerable<(Guid EmployeeId, decimal MatchScore, string MatchReasoning, string AdditionalSkills)> candidateInputs)
    {
        var rec = new Recommendation
        {
            TaskId = taskId,
            CreatedBy = createdById,
            RequiredSkillsSnapshot = JsonSerializer.Serialize(requiredSkillIds)
        };

        var ranked = candidateInputs
            .OrderByDescending(c => c.MatchScore)
            .Select((c, i) => RecommendationCandidate.Create(
                rec.Id, c.EmployeeId, c.MatchScore, c.MatchReasoning, rank: i + 1, c.AdditionalSkills))
            .ToList();

        rec._candidates.AddRange(ranked);
        return rec;
    }

    public void ApproveCandidate(Guid employeeId, Guid reviewedBy)
    {
        if (reviewedBy != CreatedBy)
        {
            throw new RecommendationAccessDeniedException(Id,reviewedBy);
        }

        var targetCandidate = GetCandidate(employeeId);

        targetCandidate.MarkAsApproved();

        foreach (var candidate in _candidates.Where(c => c.EmployeeId != employeeId))
        {
            if (candidate.Status == CandidateStatus.Pending)
            {
                candidate.MarkAsRejected();
            }
        }
    }

    public void RejectCandidate(Guid employeeId, Guid reviewedBy)
    {
        if (reviewedBy != CreatedBy)
        {
            throw new RecommendationAccessDeniedException(Id,reviewedBy);
        }

        var targetCandidate = GetCandidate(employeeId);

        targetCandidate.MarkAsRejected();
    }

    private RecommendationCandidate GetCandidate(Guid employeeId)
    {
        return _candidates.FirstOrDefault(c => c.EmployeeId == employeeId)
            ?? throw new CandidateNotPartOfRecommendationException(Id, employeeId);
    }
}
