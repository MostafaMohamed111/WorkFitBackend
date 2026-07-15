using WorkFit.Assessments.Domain.Enums;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed record ApproveAssessmentCommand(Guid AssessmentId, string? Note) : IRequest<Guid>;

