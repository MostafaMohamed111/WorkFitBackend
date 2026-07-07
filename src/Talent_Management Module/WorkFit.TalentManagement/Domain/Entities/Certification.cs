using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class Certification : BaseEntity
{
    public Guid DocumentId { get; private set; } // ref for the doc module
    public Guid EmployeeProfileId { get; private set; }
    public string Name { get; private set; } = default!;
    public string IssuingOrganization { get; private set; } = default!;
    public DateTime IssueDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }


    // Computed — مش متخزن في الداتابيز، دايماً محدث
    public bool IsExpired =>
        ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

    public EmployeeProfile Employee { get; private set; } = default!;

    public static Certification Create(Guid documentId, Guid empId, string name,
        string issuer, DateTime issueDate,
        DateTime? expiry) => new()
        {
            DocumentId = documentId,
            EmployeeProfileId = empId,
            Name = name,
            IssuingOrganization = issuer,
            IssueDate = issueDate,
            ExpiryDate = expiry,
        };

    public void Update(Guid newDocumentId,string name, string issuer,
        DateTime issueDate, DateTime? expiry, string? url)
    {
        DocumentId = newDocumentId;
        Name = name;
        IssuingOrganization = issuer;
        IssueDate = issueDate;
        ExpiryDate = expiry;
        MarkUpdated();
    }

}