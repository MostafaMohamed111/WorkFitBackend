using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

public class Certification : BaseEntity
{ 
    public Guid EmployeeId { get; private set; }
    public string Name { get; private set; } = default!;
    public string IssuingOrganization { get; private set; } = default!;
    public DateTime IssueDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? CertificateUrl { get; private set; }

    // Computed — مش متخزن في الداتابيز، دايماً محدث
    public bool IsExpired =>
        ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

    public Employee Employee { get; private set; } = default!;

    public static Certification Create(Guid empId, string name,
        string issuer, DateTime issueDate,
        DateTime? expiry, string? url) => new()
        {
            EmployeeId = empId,
            Name = name,
            IssuingOrganization = issuer,
            IssueDate = issueDate,
            ExpiryDate = expiry,
            CertificateUrl = url
        };

    public void Update(string name, string issuer,
        DateTime issueDate, DateTime? expiry, string? url)
    {
        Name = name;
        IssuingOrganization = issuer;
        IssueDate = issueDate;
        ExpiryDate = expiry;
        CertificateUrl = url;
    }
}