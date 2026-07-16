using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsByTeamLeadId;

internal sealed record GetAssessmentsByTeamLeadIdQuery(Guid TeamLeadId) : IRequest<List<AssessmentDto>>;
