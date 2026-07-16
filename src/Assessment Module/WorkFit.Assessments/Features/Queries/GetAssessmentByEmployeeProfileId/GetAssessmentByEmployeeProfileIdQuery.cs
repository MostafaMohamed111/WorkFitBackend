using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentByEmployeeProfileId;

internal sealed record GetAssessmentByEmployeeProfileIdQuery(Guid EmployeeProfileId) : IRequest<AssessmentDto>;
