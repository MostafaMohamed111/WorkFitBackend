using Microsoft.EntityFrameworkCore;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetTaskRecommendations;

public sealed class GetTaskRecommendationsQueryHandler
    : IRequestHandler<GetTaskRecommendationsQuery, List<RecommendationListItemDto>>
{
    private readonly RecommendationDbContext _context;

    public GetTaskRecommendationsQueryHandler(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecommendationListItemDto>> Handle(
        GetTaskRecommendationsQuery query,
        CancellationToken ct)
    {
        return await _context.Recommendations
            .AsNoTracking()
            .Where(r => r.TaskId == query.TaskId)
            .OrderByDescending(r => r.GeneratedAt)
            .Select(r => new RecommendationListItemDto(
                r.Id,
                r.TaskId,
                r.GeneratedAt,
                r.Candidates.Count))
            .ToListAsync(ct);
    }
}
