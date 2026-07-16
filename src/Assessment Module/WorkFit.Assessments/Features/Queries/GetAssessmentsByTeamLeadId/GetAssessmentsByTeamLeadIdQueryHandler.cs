using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsByTeamLeadId;

internal sealed class GetAssessmentsByTeamLeadIdQueryHandler : IRequestHandler<GetAssessmentsByTeamLeadIdQuery, List<AssessmentDto>>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetAssessmentsByTeamLeadIdQueryHandler(AssessmentDbContext context,
            ICurrentUserContext currentUserContext
        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }
    public async Task<List<AssessmentDto>> Handle(GetAssessmentsByTeamLeadIdQuery query, CancellationToken cancellationToken = default)
    {
        var assessments = await _context.Assessments.AsNoTracking()
            .Where(a => a.TeamLeadId == query.TeamLeadId && a.Type == AssessmentType.TeamLeadAssessment)
            .ToListAsync(cancellationToken);

        foreach (var assessment in assessments)
            assessment.ValidateAuthority(_currentUserContext.GetUserId());

        return assessments.Select(a => new AssessmentDto(
            a.Id, a.EmployeeProfileId, a.TaskId,
            a.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()
            )).ToList();
    }
}