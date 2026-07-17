using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Commands.RejectAssessment;

internal sealed record RejectAssessmentCommand(Guid AssessmentId, string? Note) : IRequest<Guid>;
