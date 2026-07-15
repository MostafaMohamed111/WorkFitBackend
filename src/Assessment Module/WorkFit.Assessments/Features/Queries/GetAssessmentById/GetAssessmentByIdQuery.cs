using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentById;

internal sealed record GetAssessmentByIdQuery(Guid AssessmentId) : IRequest<AssessmentDto>;
