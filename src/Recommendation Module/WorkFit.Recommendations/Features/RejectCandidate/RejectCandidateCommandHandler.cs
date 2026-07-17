using Microsoft.EntityFrameworkCore;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.RejectCandidate;

public sealed class RejectCandidateCommandHandler : IRequestHandler<RejectCandidateCommand>
{
    private readonly RecommendationDbContext _context;

    public RejectCandidateCommandHandler(RecommendationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectCandidateCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await _context.Recommendations
            .Include(r => r.Candidates)
            .FirstOrDefaultAsync(r => r.Id == request.RecommendationId, cancellationToken);

        if (recommendation is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Recommendation", request.RecommendationId);
        }

        recommendation.RejectCandidate(request.EmployeeId, request.ActionedBy);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
