using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Commands.RejectAssessment;

internal sealed class RejectAssessmentCommandHandler : IRequestHandler<RejectAssessmentCommand, Guid>
{
    private readonly AssessmentDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IMediator _mediator;

    public RejectAssessmentCommandHandler(
            AssessmentDbContext dbContext,
            ICurrentUserContext currentUserContext,
            IMediator mediator
    )
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(RejectAssessmentCommand command, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments.FindAsync(new object[] { command.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), command.AssessmentId);

        assessment.Reject(_currentUserContext.GetUserId(), command.Note);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new AssessmentRejectedIntegrationEvent(assessment.Id));

        return assessment.Id;
    }
}
