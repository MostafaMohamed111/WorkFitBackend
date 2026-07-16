using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.AlterAssessment;

internal sealed class AlterAssessmentCommandHandler : IRequestHandler<AlterAssessmentCommand, Guid>
{
    private readonly AssessmentDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IMediator _mediator;

    public AlterAssessmentCommandHandler(
            AssessmentDbContext dbContext,
            ICurrentUserContext currentUserContext,
            IMediator mediator
    )
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(AlterAssessmentCommand command, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments.FindAsync(new object[] { command.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), command.AssessmentId);

        var updates = command.SkillChanges
            .Select(sc => (skillChangeId: sc.SkillChangeId, newScore: sc.NewScore, note: sc.Note))
            .ToList();

        assessment.Alter(_currentUserContext.GetUserId(), updates, command.Note);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // keep score propagation pipeline aligned with approve behavior
        await _mediator.Publish(new AssessmentApprovedIntegrationEvent(
            assessment.Id,
            assessment.EmployeeProfileId,
            assessment.SkillChanges.Select(
                sc => new Change(
                    sc.SkillId,
                    sc.NewScore,
                    sc.EvidenceDescription
                    )
                ).ToList()
            ));

        return assessment.Id;
    }
}
