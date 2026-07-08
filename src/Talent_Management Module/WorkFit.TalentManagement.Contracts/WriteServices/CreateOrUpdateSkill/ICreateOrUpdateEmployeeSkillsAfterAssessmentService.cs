namespace WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;

public interface ICreateOrUpdateEmployeeSkillsAfterAssessmentService
{
    // takes emp id, skills id, new score and updates the employee's skills score in the system.
    Task CreateOrUpdateAsync(Guid employeeId, Guid assessmentId, List<SkillDetails> skills, CancellationToken cancellationToken = default);

}
