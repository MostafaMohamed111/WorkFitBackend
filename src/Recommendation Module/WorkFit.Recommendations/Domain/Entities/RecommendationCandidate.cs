using WorkFit.Recommendations.Domain.Enums;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Recommendations.Domain.Entities;

public sealed class RecommendationCandidate : BaseEntity
{
    public Guid RecommendationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal MatchScore { get; private set; }
    public string MatchReasoning { get; private set; } = default!;
    public int Rank { get; private set; }
    
    public CandidateStatus Status { get; private set; } = CandidateStatus.Pending;
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }

    private RecommendationCandidate() { }

    public static RecommendationCandidate Create(
        Guid recommendationId,
        Guid employeeId,
        decimal score,
        string matchReasoning,
        int rank)
        => new()
        {
            RecommendationId = recommendationId,
            EmployeeId = employeeId,
            MatchScore = score,
            MatchReasoning = matchReasoning,
            Rank = rank,
            Status = CandidateStatus.Pending
        };

    internal void MarkAsApproved(Guid actionedBy)
    {
        Status = CandidateStatus.Approved;
        ApprovedBy = actionedBy;
        ApprovedAt = DateTimeOffset.UtcNow;
        
        // Clear rejection data if previously rejected
        RejectedBy = null;
        RejectedAt = null;
    }

    internal void MarkAsRejected(Guid actionedBy)
    {
        Status = CandidateStatus.Rejected;
        RejectedBy = actionedBy;
        RejectedAt = DateTimeOffset.UtcNow;

        // Clear approval data if previously approved
        ApprovedBy = null;
        ApprovedAt = null;
    }
}