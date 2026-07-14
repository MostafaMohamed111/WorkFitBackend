using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed record ApproveTaskAssessmentCommand(Guid AssessmentId, string? Note) : IRequest<Guid>;

