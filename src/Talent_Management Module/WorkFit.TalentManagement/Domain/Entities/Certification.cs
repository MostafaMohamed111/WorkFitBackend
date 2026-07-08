using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class Certification : BaseEntity
{
    public Guid DocumentId { get; private set; } // ref for the doc module
    public Guid EmployeeProfileId { get; private set; }
    public string Name { get; private set; } = default!;
    public string IssuingOrganization { get; private set; } = default!;
    public DateOnly IssueDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string Details { get; private set; } = default!;


    // Computed — مش متخزن في الداتابيز، دايماً محدث
    public bool IsExpired =>
        ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public EmployeeProfile Employee { get; private set; } = default!;

    public static Certification Create(Guid empId, Guid documentId, string name,
        string issuer, DateOnly issueDate,
        string details,
        DateOnly? expiry) 
        {
        //  validation here
            return new Certification()
            {
                DocumentId = documentId,
                EmployeeProfileId = empId,
                Name = name,
                IssuingOrganization = issuer,
                IssueDate = issueDate,
                ExpiryDate = expiry,
                Details = details
            };
        }

    public void DeleteCertificate() => MarkDeleted();

}