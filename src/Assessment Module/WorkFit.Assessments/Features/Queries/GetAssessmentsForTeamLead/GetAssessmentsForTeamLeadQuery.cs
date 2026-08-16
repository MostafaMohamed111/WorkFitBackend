using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsForTeamLead;

internal sealed record GetAssessmentsForTeamLeadQuery : IRequest<List<AssessmentDto>>;
