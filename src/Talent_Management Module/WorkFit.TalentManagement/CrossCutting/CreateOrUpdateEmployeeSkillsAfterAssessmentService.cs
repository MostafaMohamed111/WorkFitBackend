
using WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;

namespace WorkFit.TalentManagement.CrossCutting;

internal class CreateOrUpdateEmployeeSkillsAfterAssessmentService : ICreateOrUpdateEmployeeSkillsAfterAssessmentService
{
    public Task CreateOrUpdateAsync(Guid employeeId, Guid assessmentId, List<SkillDetails> skills, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
