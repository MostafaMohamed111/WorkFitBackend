
using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed class ApproveAssessmentCommandHandler : IRequestHandler<ApproveAssessmentCommand, Guid>
{
    private readonly AssessmentDbContext _dbContext;

    private readonly ICurrentUserContext _currentUserContext;
    private readonly IMediator _mediator;

    public ApproveAssessmentCommandHandler(
            AssessmentDbContext dbContext,
            ICurrentUserContext currentUserContext,
            IMediator mediator
    )   
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _mediator = mediator;
    }
    public async Task<Guid> Handle(ApproveAssessmentCommand command, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments.FindAsync(new object[] { command.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), command.AssessmentId);

        assessment.Approve(_currentUserContext.GetUserId(), command.Note);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // this should be processed in the domain as domain events
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

            )); // update emp skills scores, activate emp as well if not active it might be emp self assessment
        
        return assessment.Id;

    }
}
