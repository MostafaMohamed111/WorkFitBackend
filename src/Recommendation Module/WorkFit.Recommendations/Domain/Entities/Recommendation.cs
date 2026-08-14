using System.Text.Json;
using WorkFit.Recommendations.Domain.Enums;
using WorkFit.Recommendations.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.Recommendations.Domain.Entities;

public sealed class Recommendation : BaseEntity
{
    public Guid TaskId { get; private set; } // ref to task
    public string RequiredSkillsSnapshot { get; private set; } = default!;
    public Guid CreatedBy { get; private set; } // ref to task team lead

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

    public static Recommendation CreateRanked(
        Guid taskId,
        Guid createdById,
        IReadOnlyList<Guid> requiredSkillIds,
        IReadOnlyList<RankedCandidateInput> candidateInputs)
    {
        ArgumentNullException.ThrowIfNull(requiredSkillIds);
        ArgumentNullException.ThrowIfNull(candidateInputs);

        if (taskId == Guid.Empty)
            throw new ArgumentException("Task ID must not be empty.", nameof(taskId));

        if (createdById == Guid.Empty)
            throw new ArgumentException("Creator ID must not be empty.", nameof(createdById));

        if (requiredSkillIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Required skill IDs must not be empty.", nameof(requiredSkillIds));

        if (requiredSkillIds.Count != requiredSkillIds.Distinct().Count())
            throw new ArgumentException("Required skill IDs must be unique.", nameof(requiredSkillIds));

        if (candidateInputs.Count == 0)
            throw new ArgumentException("At least one candidate is required.", nameof(candidateInputs));

        if (candidateInputs.Any(candidate => candidate.EmployeeId == Guid.Empty))
            throw new ArgumentException("Employee IDs must not be empty.", nameof(candidateInputs));

        if (candidateInputs.Select(candidate => candidate.EmployeeId).Distinct().Count() != candidateInputs.Count)
            throw new ArgumentException("Employee IDs must be unique.", nameof(candidateInputs));

        if (candidateInputs.Select(candidate => candidate.Rank).Distinct().Count() != candidateInputs.Count)
            throw new ArgumentException("Candidate ranks must be unique.", nameof(candidateInputs));

        var expectedRanks = Enumerable.Range(1, candidateInputs.Count);
        if (!candidateInputs.Select(candidate => candidate.Rank).Order().SequenceEqual(expectedRanks))
            throw new ArgumentException("Candidate ranks must be contiguous and start at 1.", nameof(candidateInputs));

        if (candidateInputs.Any(candidate => candidate.Score is < 0 or > 100))
            throw new ArgumentOutOfRangeException(nameof(candidateInputs), "Candidate scores must be between 0 and 100.");

        foreach (var candidate in candidateInputs)
        {
            if (string.IsNullOrWhiteSpace(candidate.Reasoning))
                throw new ArgumentException("Candidate reasoning must not be empty.", nameof(candidateInputs));

            ArgumentNullException.ThrowIfNull(candidate.ScoreBreakdown);

            if (candidate.ScoreBreakdown.Any(component => string.IsNullOrWhiteSpace(component.Name)))
                throw new ArgumentException("Score component names must not be empty.", nameof(candidateInputs));

            if (candidate.ScoreBreakdown
                .Select(component => component.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != candidate.ScoreBreakdown.Count)
            {
                throw new ArgumentException("Score component names must be unique per candidate.", nameof(candidateInputs));
            }

            if (candidate.ScoreBreakdown.Any(component => component.Score is < 0 or > 100))
                throw new ArgumentOutOfRangeException(nameof(candidateInputs), "Score components must be between 0 and 100.");
        }

        var recommendation = new Recommendation
        {
            TaskId = taskId,
            CreatedBy = createdById,
            RequiredSkillsSnapshot = JsonSerializer.Serialize(requiredSkillIds)
        };

        recommendation._candidates.AddRange(candidateInputs
            .OrderBy(candidate => candidate.Rank)
            .Select(candidate => RecommendationCandidate.CreateRanked(
                recommendation.Id,
                candidate.EmployeeId,
                candidate.Score,
                candidate.Reasoning.Trim(),
                candidate.Rank,
                JsonSerializer.Serialize(candidate.ScoreBreakdown))));

        return recommendation;
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

public sealed record RankedCandidateInput(
    Guid EmployeeId,
    int Rank,
    decimal Score,
    string Reasoning,
    IReadOnlyList<ScoreComponentInput> ScoreBreakdown);

public sealed record ScoreComponentInput(string Name, decimal Score);
