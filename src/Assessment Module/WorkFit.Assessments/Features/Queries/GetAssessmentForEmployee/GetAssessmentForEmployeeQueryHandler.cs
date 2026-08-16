using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentForEmployee;

internal sealed class GetAssessmentForEmployeeQueryHandler : IRequestHandler<GetAssessmentForEmployeeQuery, AssessmentDto>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetAssessmentForEmployeeQueryHandler(
            AssessmentDbContext context,
            ICurrentUserContext currentUserContext
        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<AssessmentDto> Handle(GetAssessmentForEmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var employeeUserId = _currentUserContext.GetUserId(cancellationToken);

        var assessment = await _context.Assessments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeUserId == employeeUserId
            && a.Type == Domain.Enums.AssessmentType.EmployeeProfileSelfAssessment
            && a.Status == Domain.Enums.AssessmentStatus.Pending
            , cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), employeeUserId);

        return new AssessmentDto(
            assessment.Id,
            assessment.EmployeeProfileId,
            assessment.TaskId,
            assessment.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()
            );
    }
}
