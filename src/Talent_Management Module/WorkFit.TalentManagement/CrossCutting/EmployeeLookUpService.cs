using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.LookUpServices;
using WorkFit.TalentManagement.Contracts.Dtos;
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

    public async Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await LoadEmployeesQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return employee is null ? null : Map(employee);
    }

    public async Task<List<EmployeeDetailsDto>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
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

    private static EmployeeDetailsDto Map(EmployeeProfile employee)
    {
        return new EmployeeDetailsDto(
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
            employee.EmployeeSkills.Select(MapSkill).ToList(),
            employee.IdentityMappings.Select(MapIdentityMapping).ToList(),
            employee.Certifications.Select(MapCertification).ToList());
    }

    private static EmployeeSkillLookUpDto MapSkill(EmployeeSkill skill)
    {
        return new EmployeeSkillLookUpDto(
            skill.Id,
            skill.SkillId,
            skill.SkillName,
            skill.ConfidenceScore,
            skill.ConfidenceChanges
                .OrderBy(change => change.CreatedAt)
                .Select(MapConfidenceChange)
                .ToList());
    }

    private static SkillConfidenceChangeDetailsDto MapConfidenceChange(SkillConfidenceChange change)
    {
        return new SkillConfidenceChangeDetailsDto(
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

    private static ConfidenceEvidenceDetailsDto MapEvidence(ConfidenceEvidence evidence)
    {
        return new ConfidenceEvidenceDetailsDto(
            evidence.Id,
            evidence.Source,
            evidence.Details,
            evidence.EvidenceDate,
            evidence.CreatedAt,
            evidence.UpdatedAt);
    }

    private static EmployeeIdentityMappingDetailsDto MapIdentityMapping(DeveloperIdentityMapping mapping)
    {
        return new EmployeeIdentityMappingDetailsDto(
            mapping.Id,
            mapping.SourceSystem,
            mapping.ExternalAccountId,
            mapping.ExternalDisplayName,
            mapping.CreatedAt,
            mapping.UpdatedAt);
    }

    private static EmployeeCertificationDetailsDto MapCertification(Certification certification)
    {
        return new EmployeeCertificationDetailsDto(
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
