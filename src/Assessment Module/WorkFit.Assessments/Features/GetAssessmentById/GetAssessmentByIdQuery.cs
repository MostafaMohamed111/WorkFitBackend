using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.GetAssessmentById;

internal sealed record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<AssessmentDto>;
