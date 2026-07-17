using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentByEmployeeProfileId;

internal sealed class GetAssessmentByEmployeeProfileIdQueryHandler : IRequestHandler<GetAssessmentByEmployeeProfileIdQuery, AssessmentDto>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetAssessmentByEmployeeProfileIdQueryHandler(
            AssessmentDbContext context,
            ICurrentUserContext currentUserContext
        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<AssessmentDto> Handle(GetAssessmentByEmployeeProfileIdQuery query, CancellationToken cancellationToken = default)
    {
        var assessment = await _context.Assessments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeProfileId == query.EmployeeProfileId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), query.EmployeeProfileId);

        assessment.ValidateAuthority(_currentUserContext.GetUserId());

        return new AssessmentDto(
            assessment.Id,
            assessment.EmployeeProfileId,
            assessment.TaskId,
            assessment.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()
            );
    }
}
