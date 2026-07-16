using Microsoft.EntityFrameworkCore;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentById;

internal sealed class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDto>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetAssessmentByIdQueryHandler(AssessmentDbContext context,
            ICurrentUserContext currentUserContext

        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }
    public async Task<AssessmentDto> Handle(GetAssessmentByIdQuery query, CancellationToken cancellationToken = default)
    {
        var assessmet = await _context.Assessments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == query.AssessmentId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), query.AssessmentId);
        
        assessmet.ValidateAuthority(_currentUserContext.GetUserId());

        return new AssessmentDto(assessmet.Id, assessmet.EmployeeProfileId, assessmet.TaskId,
                assessmet.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()
        );
        
    }
}