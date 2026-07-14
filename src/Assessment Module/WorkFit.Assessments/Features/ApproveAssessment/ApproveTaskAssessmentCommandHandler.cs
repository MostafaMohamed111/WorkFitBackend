
using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed class ApproveTaskAssessmentCommandHandler : IRequestHandler<ApproveTaskAssessmentCommand, Guid>
{
    private readonly AssessmentDbContext _dbContext;

    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITaskLookUpService _taskLookUpService;
    private readonly IMediator _mediator;

    public ApproveTaskAssessmentCommandHandler(
            AssessmentDbContext dbContext,
            ICurrentUserContext currentUserContext,
            ITaskLookUpService taskLookUpService,
            IMediator mediator)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _taskLookUpService = taskLookUpService;
        _mediator = mediator;
    }
    public async Task<Guid> Handle(ApproveTaskAssessmentCommand command, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments.FindAsync(new object[] { command.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), command.AssessmentId);

        // taskid will never be null enforced by domain factory method
        var task = await _taskLookUpService.GetTaskByIdAsync((Guid)assessment.TaskId!);

        if(_currentUserContext.GetUserId() != task.TeamLeadId)
            throw new ForbiddenAccessException(ModuleMarker.ModuleName,typeof(Assessment).ToString());

        assessment.Approve(_currentUserContext.GetUserId(), command.Note);

        _dbContext.SaveChanges();

        await _mediator.Publish(new AssessmentApprovedIntegrationEvent(assessment.Id, (Guid)assessment.TaskId!, assessment.EmployeeProfileId));

        return assessment.Id;

    }
}
