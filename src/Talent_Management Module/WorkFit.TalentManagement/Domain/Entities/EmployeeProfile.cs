
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.TalentManagement.Domain.Enums;
using WorkFit.TalentManagement.Domain.Exceptions;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class EmployeeProfile : BaseEntity
{
   
    // Cross-module references (IDs only)
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public EmployeeProfileStatus Status { get; private set; } 
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? Bio { get; private set; }
    public string? LinkedInUrl { get; private set; }
    public string JobTitle { get; private set; } = default!;
    public DateOnly? HireDate { get; private set; }
    public int CurrentAllocationPercentage { get; private set; } // denormalized, synced via integration event

    // owned collections

    private readonly List<EmployeeSkill> _employeeSkills = new List<EmployeeSkill>();
    public IReadOnlyCollection<EmployeeSkill> EmployeeSkills => _employeeSkills;

    private readonly List<Certification> _certifications = new List<Certification>();
    public IReadOnlyCollection<Certification> Certifications => _certifications;

    public static EmployeeProfile Create(
        Guid orgId, Guid userId, string email, string name,
         string jobTitle, DateOnly? hireDate = null)
    {
        // Validation here
        return new EmployeeProfile()
        {
            Name = name,
            OrganizationId = orgId,
            UserId = userId,
            Email = email,
            JobTitle = jobTitle,
            HireDate = hireDate,
            Status = EmployeeProfileStatus.PendingReview
        };
    }
    public void DeactivateEmployee() => Status = EmployeeProfileStatus.Inactive;

    public void UpdateEmployeePersonalData(string? newName, string? jobTitle, string? newBio = null, string? newLinkedInUrl = null)
    {
        Name = newName ?? Name;
        JobTitle = jobTitle ?? JobTitle;
        Bio = newBio ?? Bio;
        LinkedInUrl = newLinkedInUrl ?? LinkedInUrl;
        MarkUpdated();
    }


    // This method is called when an integration event is received from the project management module.
    public void UpdateAllocation(int newPercentage)
    {
        CurrentAllocationPercentage = newPercentage;
        MarkUpdated();
    }

    public bool IsActive() => Status == EmployeeProfileStatus.Active;


    // aggregate root controls childeren entities
    // adding a skill for the first time based on an assessment
    public void AddEmployeeSkill( Guid skillId, Guid assessmentId,
        string skillName, int confidenceScore, string details, string source)
    {
        var existingSkill = _employeeSkills.FirstOrDefault(s => s.SkillId == skillId);
        if (existingSkill != null)
            throw new DuplicateEmployeeSkillInsertionDomainException();
        // validation within the childeren entity factory method
        var skill = EmployeeSkill.Create(Id, skillId, assessmentId, skillName, confidenceScore, details, source);
        _employeeSkills.Add(skill);
        MarkUpdated();
    }


    // aggregate root controls childeren entities
    // updating an existing skill's confidence score based on an assessment
    public void UpdateEmployeeSkillConfidenceScore(Guid skillId, Guid assessmentId, int newConfidenceScore, string details, string source)
    {
        var skill = _employeeSkills.FirstOrDefault(s => s.Id == skillId)
            ?? throw new EmployeeSkillWasNotFoundException();

        skill.ApplyAssessedChange(assessmentId, newConfidenceScore, details, source);
        MarkUpdated();
    }

    // aggregate root controls childeren entities
    // adding a certification to the employee profile
    public void AddCertification(Guid documentId,
        string certName,
        string issuerOrganization,
        DateOnly issueDate,
        string details,
        DateOnly? expiryDate = null
        )
    {
        var existingCert = _certifications.FirstOrDefault(c => c.DocumentId == documentId);
        if (existingCert != null)
            throw new DuplicateCertificationInsertionDomainException();
        var cert = Certification.Create(Id, documentId, certName, issuerOrganization, issueDate, details, expiryDate);
        _certifications.Add(cert);
        MarkUpdated();
    }

    public void RemoveCertification(Guid certId)
    {
        var cert = _certifications.FirstOrDefault(c => c.Id == certId)
            ?? throw new CertificationWasNotFoundException();
        cert.DeleteCertificate();
        _certifications.Remove(cert);
        MarkUpdated();
    }



    // if an update certificate is needed would be implemented as an entry here as in the aggregate root


}
