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
    public async Task<AssessmentDto> Handle(
    GetAssessmentByIdQuery query,
    CancellationToken cancellationToken = default)
    {
        var assessment = await _context.Assessments
            .AsNoTracking()
            .Include(a => a.SkillChanges)
            .FirstOrDefaultAsync(
                a => a.Id == query.AssessmentId,
                cancellationToken
            )
            ?? throw new EntityNotFoundException(
                ModuleMarker.ModuleName,
                typeof(Assessment).ToString(),
                query.AssessmentId
            );

        assessment.ValidateAuthority(_currentUserContext.GetUserId());

        return new AssessmentDto(
            assessment.Id,
            assessment.EmployeeProfileId,
            assessment.TaskId,
            assessment.SkillChanges
                .Select(sc => new SkillChangeDto(
                    sc.Id,
                    sc.SkillId, 
                    sc.SkillName,
                    sc.OldScore,
                    sc.ProposedScore,
                    sc.EvidenceDescription
                    )
                )
                .ToList(),
            assessment.Status
        );
    }
}