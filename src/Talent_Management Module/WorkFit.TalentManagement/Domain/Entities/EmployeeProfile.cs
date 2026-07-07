
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.TalentManagement.Domain.Enums;

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

    public void UpdateDetails(string? newName, string? jobTitle, string? newBio = null, string? newLinkedInUrl = null)
    {
        Name = newName ?? Name;
        JobTitle = jobTitle ?? JobTitle;
        Bio = newBio ?? Bio;
        LinkedInUrl = newLinkedInUrl ?? LinkedInUrl;
        MarkUpdated();
    }

    public void UpdateAllocation(int newPercentage)
    {
        CurrentAllocationPercentage = newPercentage;
        MarkUpdated();
    }

    public bool IsActive() => Status == EmployeeProfileStatus.Active;
}
