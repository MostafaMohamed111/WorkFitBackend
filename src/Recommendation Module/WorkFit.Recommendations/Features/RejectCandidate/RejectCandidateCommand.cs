using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.RejectCandidate;

public sealed record RejectCandidateCommand(
    Guid RecommendationId,
    Guid EmployeeId,
    Guid ActionedBy
) : IRequest;
