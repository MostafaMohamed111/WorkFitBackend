
using WorkFit.Assessments.Domain.Enums;
using WorkFit.Assessments.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Assessments.Domain.Entities;

internal sealed class Assessment : BaseEntity
{
    public Guid? ProcessedById { get; private set; }
    public Guid EmployeeProfileId { get; private set; }
    public Guid? TaskId { get; private set; }
    public string Description { get; private set; } = default!;

    private readonly List<SkillChange> _skillChanges = new();
    public IReadOnlyCollection<SkillChange> SkillChanges => _skillChanges;
    public AssessmentStatus Status { get; private set; }
    public AssessmentType Type { get; private set; }
    public string? ProcessorNote { get; private set; } = null;


    private Assessment() { } // EF

    private Assessment(Guid employeeProfileId, string description, AssessmentType type, Guid? taskId)
    {
        EmployeeProfileId = employeeProfileId;
        Description = description;
        Type = type;
        Status = AssessmentStatus.Pending;
        TaskId = TaskId;
    }

    public static Assessment Create(Guid employeeProfileId, string description, AssessmentType type, List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)> skillChanges, Guid? taskId)
    {
        // validation 
        // task assessment must have a task id
        if (type == AssessmentType.TeamLeadAssessment && taskId is null)
            throw new FeildIsNullOrEmptyException(ModuleMarker.ModuleName, typeof(Assessment).ToString(), nameof(taskId));

        // self profile assessment should not have a task id
        if (type == AssessmentType.EmployeeProfileSelfAssessment && taskId is not null)
            throw new InvalidEmployeeProfileSelfAssessmentTaskIdMustBeNullException();

        var assessment = new Assessment(employeeProfileId, description, type, taskId);

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

        ProcessedById = processedById;
        Status = AssessmentStatus.Approved;
        ProcessorNote = teamLeadNote;
        MarkUpdated();
    }

    public void Reject(Guid processedById, string? teamLeadNote = null)
    {
        if (Status != AssessmentStatus.Pending)
            throw new InvalidAssessmentStatusTransitionDomainException(Status, AssessmentStatus.Rejected);
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

}