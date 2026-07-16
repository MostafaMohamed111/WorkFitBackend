
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Entities;

internal sealed class Assessment : BaseEntity
{
    public Guid? ProcessedById { get; private set; } // ref to user accessing the assessment for approval/rejection/alteration identity
    public Guid EmployeeProfileId { get; private set; } // ref to emps
    public Guid EmployeeUserId { get; private set; } // ref to employee user id identity
    public Guid? TaskId { get; private set; } // ref to tasks
    public Guid? TeamLeadId { get; private set; } // ref to team lead user id identity
    public string Description { get; private set; } = default!;

    private readonly List<SkillChange> _skillChanges = new();
    public IReadOnlyCollection<SkillChange> SkillChanges => _skillChanges;
    public AssessmentStatus Status { get; private set; }
    public AssessmentType Type { get; private set; }
    public string? ProcessorNote { get; private set; } = null;


    private Assessment() { } // EF

    private Assessment(Guid employeeProfileId, Guid employeeUserId, string description, AssessmentType type, Guid? taskId = null, Guid? teamLeadId = null)
    {
        EmployeeProfileId = employeeProfileId;
        EmployeeUserId = employeeUserId;
        Description = description;
        Type = type;
        Status = AssessmentStatus.Pending;
        TaskId = taskId;
        TeamLeadId = teamLeadId;
    }

    public static Assessment Create(Guid employeeProfileId, Guid employeeUserId, string description, AssessmentType type, List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)> skillChanges, Guid? taskId = null, Guid? teamLeadId = null)
    {
        // validation 
        // task assessment must have a task id
        if (type == AssessmentType.TeamLeadAssessment)
        {
            if (taskId is null)
                throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), nameof(taskId));
            if(teamLeadId is null)
                throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), nameof(teamLeadId));
        }
        // self profile assessment should not have a task id
        if (type == AssessmentType.EmployeeProfileSelfAssessment )
        {
            if(taskId is not null)
                throw new InvalidEmployeeProfileSelfAssessmentTaskIdMustBeNullException();
            if(teamLeadId is not null)
                throw new InvalidEmployeeProfileSelfAssessmentTeamLeadIdMustBeNullException();
        }

        var assessment = new Assessment(employeeProfileId, employeeUserId, description, type, taskId, teamLeadId);

        foreach (var skillChange in skillChanges)
        {
            foreach (var existingSkillChange in assessment._skillChanges)
            {
                if (existingSkillChange.SkillId == skillChange.skillId)
                    throw new DuplicateAssessmentSkillChangeDomainException(skillChange.skillName);

            }
            assessment._skillChanges.Add(SkillChange.Create(assessment.Id, skillChange.skillId, skillChange.skillName, skillChange.oldScore, skillChange.proposedScore, skillChange.evidenceDesc));
        }

        return assessment;
    }


    public void Approve(Guid processedById, string? teamLeadNote = null)
    {
        if (Status != AssessmentStatus.Pending)
            throw new InvalidAssessmentStatusTransitionDomainException(Status, AssessmentStatus.Approved);

        ValidateAuthority(processedById);

        ProcessedById = processedById;
        Status = AssessmentStatus.Approved;
        ProcessorNote = teamLeadNote;
        MarkUpdated();
    }

    public void Reject(Guid processedById, string? teamLeadNote = null)
    {
        if (Status != AssessmentStatus.Pending)
            throw new InvalidAssessmentStatusTransitionDomainException(Status, AssessmentStatus.Rejected);

        ValidateAuthority(processedById);

        ProcessedById = processedById;
        Status = AssessmentStatus.Rejected;
        ProcessorNote = teamLeadNote;
        MarkUpdated();
    }

    public void Alter(Guid processedById, List<(Guid skillChangeId, int newScore, string note)> skillChangesUpdates , string? teamLeadNote = null)
    {
        // validation
        if (Status != AssessmentStatus.Pending)
            throw new InvalidAssessmentStatusTransitionDomainException(Status, AssessmentStatus.Altered);
        
        ValidateAuthority(processedById);
    
        foreach (var update in skillChangesUpdates)
        {
            var skillChange = _skillChanges.FirstOrDefault(sc => sc.Id == update.skillChangeId);
            if (skillChange == null)
                throw new SkillChangeNotFoundDomainException(update.skillChangeId);

            skillChange.UpdateScore(update.newScore, update.note);
        }
        ProcessedById = processedById;
        ProcessorNote = teamLeadNote;
        Status = AssessmentStatus.Altered;
        MarkUpdated();
    }


    public void ValidateAuthority(Guid processedById)
    {
        // check identity ownership
        if(Type == AssessmentType.TeamLeadAssessment)
            if(processedById != TeamLeadId)
                throw new AssessmentUnAuthorizedActionDomainException(processedById);

        if(Type == AssessmentType.EmployeeProfileSelfAssessment)
            if(processedById != EmployeeUserId)
                throw new AssessmentUnAuthorizedActionDomainException(processedById);
    }
}