using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.LookUpServices;
using WorkFit.TalentManagement.CrossCutting.Dtos;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class EmployeeLookUpService : IEmployeeLookUpService
{
    private readonly TalentDbContext _db;

    public EmployeeLookUpService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeAiDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await LoadEmployeesQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return employee is null ? null : Map(employee);
    }

    public async Task<List<EmployeeAiDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        var employees = await LoadEmployeesQuery()
            .ToListAsync(cancellationToken);

        return employees.Select(Map).ToList();
    }

    private IQueryable<EmployeeProfile> LoadEmployeesQuery()
    {
        return _db.EmployeeProfiles
            .AsNoTracking()
            .Include(e => e.EmployeeSkills)
                .ThenInclude(s => s.ConfidenceChanges)
                    .ThenInclude(c => c.ConfidenceEvidences)
            .Include(e => e.IdentityMappings)
            .Include(e => e.Certifications)
            .Where(e => !e.IsDeleted);
    }

    private static EmployeeAiDto Map(EmployeeProfile employee)
    {
        return new EmployeeAiDto(
            employee.Id,
            employee.OrganizationId,
            employee.UserId,
            employee.Name,
            employee.Email,
            employee.Bio,
            employee.LinkedInUrl,
            employee.JobTitle,
            employee.Status.ToString(),
            employee.IsActive(),
            employee.CurrentAllocationPercentage,
            employee.HireDate,
            employee.CreatedAt,
            employee.UpdatedAt,
            employee.EmployeeSkills
                .Select(MapSkill)
                .ToList(),
            employee.IdentityMappings
                .Select(MapIdentityMapping)
                .ToList(),
            employee.Certifications
                .Select(MapCertification)
                .ToList());
    }

    private static EmployeeSkillAiDto MapSkill(EmployeeSkill skill)
    {
        return new EmployeeSkillAiDto(
            skill.Id,
            skill.SkillId,
            skill.SkillName,
            skill.ConfidenceScore,
            skill.ConfidenceChanges
                .OrderBy(change => change.CreatedAt)
                .Select(MapConfidenceChange)
                .ToList());
    }

    private static SkillConfidenceChangeAiDto MapConfidenceChange(SkillConfidenceChange change)
    {
        return new SkillConfidenceChangeAiDto(
            change.Id,
            change.AssessmentId,
            change.OldScore,
            change.NewScore,
            change.CreatedAt,
            change.ConfidenceEvidences
                .OrderBy(evidence => evidence.CreatedAt)
                .Select(MapEvidence)
                .ToList());
    }

    private static ConfidenceEvidenceAiDto MapEvidence(ConfidenceEvidence evidence)
    {
        return new ConfidenceEvidenceAiDto(
            evidence.Id,
            evidence.Source,
            evidence.Details,
            evidence.EvidenceDate,
            evidence.CreatedAt,
            evidence.UpdatedAt);
    }

    private static EmployeeIdentityMappingAiDto MapIdentityMapping(DeveloperIdentityMapping mapping)
    {
        return new EmployeeIdentityMappingAiDto(
            mapping.Id,
            mapping.SourceSystem,
            mapping.ExternalAccountId,
            mapping.ExternalDisplayName,
            mapping.CreatedAt,
            mapping.UpdatedAt);
    }

    private static EmployeeCertificationAiDto MapCertification(Certification certification)
    {
        return new EmployeeCertificationAiDto(
            certification.Id,
            certification.DocumentId,
            certification.Name,
            certification.IssuingOrganization,
            certification.IssueDate,
            certification.ExpiryDate,
            certification.Details,
            certification.IsExpired,
            certification.CreatedAt,
            certification.UpdatedAt);
    }
}
