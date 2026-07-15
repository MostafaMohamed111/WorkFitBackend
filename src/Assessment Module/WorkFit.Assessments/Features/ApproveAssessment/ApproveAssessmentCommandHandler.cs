
using WorkFit.Assessments.Contracts.IntegrationEvents;
using WorkFit.Assessments.Domain.DomainServices;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed class ApproveAssessmentCommandHandler : IRequestHandler<ApproveAssessmentCommand, Guid>
{
    private readonly AssessmentDbContext _dbContext;

    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITaskLookUpService _taskLookUpService;
    private readonly IMediator _mediator;
    private readonly AssessmentAuthorityService _assessmentAuthorityService;

    public ApproveAssessmentCommandHandler(
            AssessmentDbContext dbContext,
            ICurrentUserContext currentUserContext,
            ITaskLookUpService taskLookUpService,
            IMediator mediator,
            AssessmentAuthorityService assessmentAuthorityService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _taskLookUpService = taskLookUpService;
        _mediator = mediator;
        _assessmentAuthorityService = assessmentAuthorityService;
    }
    public async Task<Guid> Handle(ApproveAssessmentCommand command, CancellationToken cancellationToken = default)
    {
        var assessment = await _dbContext.Assessments.FindAsync(new object[] { command.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), command.AssessmentId);

        Guid? teamleadId = null;
        if(assessment.Type == AssessmentType.TeamLeadAssessment)
        {
            // task id will never be null enforced by domain factory method
            var task = await _taskLookUpService.GetTaskByIdAsync((Guid)assessment.TaskId!);
            teamleadId = task.TeamLeadId;
        }

        _assessmentAuthorityService.Validate(assessment, _currentUserContext.GetUserId(), teamleadId);

        assessment.Approve(_currentUserContext.GetUserId(), command.Note);

        await _dbContext.SaveChangesAsync();

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
