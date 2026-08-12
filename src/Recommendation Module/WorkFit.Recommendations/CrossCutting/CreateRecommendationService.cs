using System.Text.Json;
using WorkFit.Recommendations.Contracts.CreateRecommendationService;
using WorkFit.Recommendations.Domain.Entities;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.Recommendations.CrossCutting;

internal sealed class CreateRecommendationService : ICreateRecommendationService
{
    private readonly RecommendationDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateRecommendationService(
        RecommendationDbContext context,
        ICurrentUserContext currentUserContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<PersistedRecommendationDto> CreateAsync(
        CreateRecommendationDto recommendation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentNullException.ThrowIfNull(recommendation.RequiredSkillIds);
        ArgumentNullException.ThrowIfNull(recommendation.Candidates);

        var candidateInputs = recommendation.Candidates
            .Select(candidate =>
            {
                ArgumentNullException.ThrowIfNull(candidate);
                ArgumentNullException.ThrowIfNull(candidate.ScoreBreakdown);

                return new RankedCandidateInput(
                    candidate.EmployeeId,
                    candidate.Rank,
                    candidate.Score,
                    candidate.Reasoning,
                    candidate.ScoreBreakdown
                        .Select(component =>
                        {
                            ArgumentNullException.ThrowIfNull(component);
                            return new ScoreComponentInput(component.Name, component.Score);
                        })
                        .ToList());
            })
            .ToList();

        var aggregate = Recommendation.CreateRanked(
            recommendation.TaskId,
            _currentUserContext.GetUserId(cancellationToken),
            recommendation.RequiredSkillIds,
            candidateInputs);

        _context.Recommendations.Add(aggregate);
        await _context.SaveChangesAsync(cancellationToken);

        var candidates = aggregate.Candidates
            .OrderBy(candidate => candidate.Rank)
            .Select(candidate => new PersistedRecommendationCandidateDto(
                candidate.Id,
                candidate.EmployeeId,
                candidate.Rank,
                candidate.MatchScore,
                JsonSerializer.Deserialize<List<RecommendationScoreComponentDto>>(candidate.ScoreBreakdown) ?? [],
                candidate.MatchReasoning))
            .ToList();

        return new PersistedRecommendationDto(
            aggregate.Id,
            aggregate.TaskId,
            aggregate.CreatedBy,
            aggregate.CreatedAt,
            candidates);
    }
}
