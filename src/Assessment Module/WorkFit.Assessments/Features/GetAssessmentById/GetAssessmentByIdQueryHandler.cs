using WorkFit.Assessments.Domain.DomainServices;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Infrastructure.Data;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.GetAssessmentById;

internal sealed class GetAssessmentByIdQueryHandler : IRequestHandler<GetAssessmentByIdQuery, AssessmentDto>
{
    private readonly AssessmentDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITaskLookUpService _taskLookUpService;
    private readonly AssessmentAuthorityService _assessmentAuthorityService;

    public GetAssessmentByIdQueryHandler(AssessmentDbContext context,
            ICurrentUserContext currentUserContext,
            ITaskLookUpService taskLookUpService,
            AssessmentAuthorityService assessmentAuthorityService

        )
    {
        _context = context;
        _currentUserContext = currentUserContext;
        _taskLookUpService = taskLookUpService;
        _assessmentAuthorityService = assessmentAuthorityService;
    }
    public async Task<AssessmentDto> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken = default)
    {
        var assessmet = await _context.Assessments.FindAsync(new object[] { request.AssessmentId }, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), request.AssessmentId);
        
        Guid? teamLeadId = null;
        if (assessmet.Type == AssessmentType.TeamLeadAssessment)
        {
            var task = await _taskLookUpService.GetTaskByIdAsync((Guid)assessmet.TaskId!, cancellationToken);
            teamLeadId = task?.TeamLeadId;
        }
        _assessmentAuthorityService.Validate(assessmet, _currentUserContext.GetUserId(), teamLeadId);

        return new AssessmentDto(assessmet.Id, assessmet.EmployeeProfileId, assessmet.TaskId,
                assessmet.SkillChanges.Select(sc => new SkillChangeDto(sc.SkillId, sc.SkillName, sc.OldScore, sc.ProposedScore, sc.EvidenceDescription)).ToList()

                );
        
    }
}