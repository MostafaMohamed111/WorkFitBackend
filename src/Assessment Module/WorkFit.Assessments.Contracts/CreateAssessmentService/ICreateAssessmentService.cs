namespace WorkFit.Assessments.Contracts.CreateAssessmentService;

public interface ICreateAssessmentService
{
    Task<Guid> CreateAsync(Guid employeeProfileId, string description, AssessmentType type, List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)> skillChanges, Guid? taskId);
}
