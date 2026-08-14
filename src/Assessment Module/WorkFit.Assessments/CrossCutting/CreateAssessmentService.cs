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
        var assessmentType = MapToDomainType(type);
        var assessment = Assessment.Create(employeeProfileId, employeeUserId, description, assessmentType, skillChanges, taskId, teamLeadId);

        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();

        return assessment.Id;
    }

    // Contracts and Domain enums have different underlying values, so map by name, not by numeric cast.
    private static Domain.Enums.AssessmentType MapToDomainType(AssessmentType type) => type switch
    {
        AssessmentType.EmployeeProfileSelfAssessment => Domain.Enums.AssessmentType.EmployeeProfileSelfAssessment,
        AssessmentType.TeamLeadAssessment => Domain.Enums.AssessmentType.TeamLeadAssessment,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown assessment type.")
    };
}
