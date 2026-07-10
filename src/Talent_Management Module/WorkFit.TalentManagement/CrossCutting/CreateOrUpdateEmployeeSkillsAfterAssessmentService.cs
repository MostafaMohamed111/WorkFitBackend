using WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;
using WorkFit.TalentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WorkFit.TalentManagement.CrossCutting;

internal class CreateOrUpdateEmployeeSkillsAfterAssessmentService : ICreateOrUpdateEmployeeSkillsAfterAssessmentService
{
    private readonly TalentDbContext _db;

    public CreateOrUpdateEmployeeSkillsAfterAssessmentService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task CreateOrUpdateAsync(Guid employeeId, Guid assessmentId, List<SkillDetails> skills, CancellationToken cancellationToken = default)
    {
        var employee = await _db.EmployeeProfiles
            .Include(e => e.EmployeeSkills)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null) return;

        foreach (var skill in skills)
        {
            employee.AddOrUpdateEmployeeSkill(skill.skillId, assessmentId, skill.skillName, skill.skillScore, skill.details, skill.source);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
