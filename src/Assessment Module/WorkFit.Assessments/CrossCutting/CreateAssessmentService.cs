using WorkFit.Assessments.Contracts.CreateAssessmentService;
using WorkFit.Assessments.Domain.Entities;
using WorkFit.Assessments.Infrastructure.Data;

namespace WorkFit.Assessments.CrossCutting;

internal sealed class CreateAssessmentService : ICreateAssessmentService
{
    private readonly AssessmentDbContext _context;

    public CreateAssessmentService(AssessmentDbContext context)
    {
        _context = context;
    }
    public async Task<Guid> CreateAsync(Guid employeeProfileId, Guid employeeUserId, string description, AssessmentType type, List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)> skillChanges, Guid? taskId = null, Guid? teamLeadId = null)
    {
        var assessmentType = (Domain.Enums.AssessmentType)type;
        var assessment = Assessment.Create(employeeProfileId, employeeUserId, description, assessmentType, skillChanges, taskId, teamLeadId);

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        return assessment.Id;
    }
}
