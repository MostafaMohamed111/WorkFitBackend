using Microsoft.EntityFrameworkCore;
using WorkFit.Recommendations.Contracts.IntegrationEvents;
using WorkFit.Recommendations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.ApproveCandidate;

public sealed class ApproveCandidateCommandHandler : IRequestHandler<ApproveCandidateCommand>
{
    private readonly RecommendationDbContext _context;
    private readonly IMediator _mediator;

    public ApproveCandidateCommandHandler(RecommendationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task Handle(ApproveCandidateCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await _context.Recommendations
            .Include(r => r.Candidates)
            .FirstOrDefaultAsync(r => r.Id == request.RecommendationId, cancellationToken);

        if (recommendation is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Recommendation", request.RecommendationId);
        }

        recommendation.ApproveCandidate(request.EmployeeId, request.ReviewedBy);

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new CandidateApprovedIntegrationEvent(recommendation.TaskId, request.EmployeeId), cancellationToken);
    }
}
