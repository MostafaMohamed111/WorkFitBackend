using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentForEmployee;

internal sealed record GetAssessmentForEmployeeQuery : IRequest<AssessmentDto>;
