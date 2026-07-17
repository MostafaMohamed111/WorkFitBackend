namespace WorkFit.Assessments.Contracts.CreateAssessmentService;

public interface ICreateAssessmentService
{
    Task<Guid> CreateAsync(Guid employeeProfileId, Guid employeeUserId, string description, AssessmentType type, List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)> skillChanges, Guid? taskId = null, Guid? teamLeadId = null);
}
