using WorkFit.Assessments.Domain.Enums;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Commands.ApproveAssessment;

internal sealed record ApproveAssessmentCommand(Guid AssessmentId, string? Note) : IRequest<Guid>;

