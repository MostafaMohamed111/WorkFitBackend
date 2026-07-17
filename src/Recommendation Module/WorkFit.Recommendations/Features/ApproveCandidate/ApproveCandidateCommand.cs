using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.ApproveCandidate;

public sealed record ApproveCandidateCommand(
    Guid RecommendationId,
    Guid EmployeeId,
    Guid ReviewedBy
) : IRequest;
