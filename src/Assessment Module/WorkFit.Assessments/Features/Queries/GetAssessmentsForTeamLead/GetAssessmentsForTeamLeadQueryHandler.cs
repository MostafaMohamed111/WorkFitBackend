using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsForTeamLead;

internal sealed class GetAssessmentsForTeamLeadQueryHandler : IRequestHandler<GetAssessmentsForTeamLeadQuery, List<AssessmentDto>>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetAssessmentsForTeamLeadQueryHandler(AssessmentDbContext context,
            ICurrentUserContext currentUserContext
        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }
    public async Task<List<AssessmentDto>> Handle(GetAssessmentsForTeamLeadQuery query, CancellationToken cancellationToken = default)
    {
        var teamLeadId = _currentUserContext.GetUserId(cancellationToken);

        var assessments = await _context.Assessments.AsNoTracking()
            .Include(a => a.SkillChanges)
            .Where(a => a.TeamLeadId == teamLeadId && a.Type == AssessmentType.TeamLeadAssessment && a.Status == AssessmentStatus.Pending)
            .ToListAsync(cancellationToken);

        return assessments.Select(a => new AssessmentDto(
            a.Id, a.EmployeeProfileId, a.TaskId,
            a.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()
            )).ToList();
    }
}
