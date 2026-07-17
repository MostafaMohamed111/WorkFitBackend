using WorkFit.Recommendations.Domain.Enums;
using WorkFit.Recommendations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Recommendations.Domain.Entities;

public sealed class RecommendationCandidate : BaseEntity
{
    public Guid RecommendationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal MatchScore { get; private set; }
    public string MatchReasoning { get; private set; } = default!;
    public int Rank { get; private set; }
    public string AdditionalSkills { get; private set; } = "[]";
    
    public CandidateStatus Status { get; private set; } = CandidateStatus.Pending;
    public DateTimeOffset? ReviewedAt { get; private set; }

    private RecommendationCandidate() { }

    public static RecommendationCandidate Create(
        Guid recommendationId,
        Guid employeeId,
        decimal score,
        string matchReasoning,
        int rank,
        string additionalSkills)
        => new()
        {
            RecommendationId = recommendationId,
            EmployeeId = employeeId,
            MatchScore = score,
            MatchReasoning = matchReasoning,
            Rank = rank,
            AdditionalSkills = additionalSkills,
            Status = CandidateStatus.Pending
        };

    internal void MarkAsApproved()
    {
        if (Status == CandidateStatus.Approved)
            throw new CandidateAlreadyApprovedException(EmployeeId);

        if (Status == CandidateStatus.Rejected)
            throw new CandidateAlreadyRejectedException(EmployeeId);

        Status = CandidateStatus.Approved;
        ReviewedAt = DateTimeOffset.UtcNow;
    }

    internal void MarkAsRejected()
    {
        if (Status == CandidateStatus.Rejected)
            throw new CandidateAlreadyRejectedException(EmployeeId);

        if (Status == CandidateStatus.Approved)
            throw new CandidateApprovalNotAllowedException(EmployeeId);

        Status = CandidateStatus.Rejected;
        ReviewedAt = DateTimeOffset.UtcNow;
    }
}