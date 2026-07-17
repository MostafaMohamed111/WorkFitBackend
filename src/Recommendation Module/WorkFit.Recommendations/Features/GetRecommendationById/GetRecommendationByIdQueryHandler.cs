using Microsoft.EntityFrameworkCore;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetRecommendationById;

public sealed class GetRecommendationByIdQueryHandler
    : IRequestHandler<GetRecommendationByIdQuery, RecommendationDetailDto>
{
    private readonly RecommendationDbContext _context;

    public GetRecommendationByIdQueryHandler(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<RecommendationDetailDto> Handle(
        GetRecommendationByIdQuery query,
        CancellationToken ct)
    {
        var recommendation = await _context.Recommendations
            .AsNoTracking()
            .Include(r => r.Candidates)
            .FirstOrDefaultAsync(r => r.Id == query.Id, ct);

        if (recommendation is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Recommendation", query.Id);

        var candidates = recommendation.Candidates
            .OrderBy(c => c.Rank)
            .Select(c => new CandidateDetailDto(
                c.EmployeeId,
                c.MatchScore,
                c.MatchReasoning,
                c.Rank,
                c.Status,
                c.ReviewedAt))
            .ToList();

        return new RecommendationDetailDto(
            recommendation.Id,
            recommendation.TaskId,
            recommendation.CreatedBy,
            recommendation.CreatedAt,
            recommendation.RequiredSkillsSnapshot,
            candidates);
    }
}
