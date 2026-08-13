using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.TalentManagement.Domain.Entities;

internal sealed class DeveloperIdentityMapping : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid EmployeeProfileId { get; private set; }
    public string SourceSystem { get; private set; } = default!;
    public string ExternalAccountId { get; private set; } = default!;
    public string ExternalDisplayName { get; private set; } = default!;

    public EmployeeProfile Employee { get; private set; } = default!;

    // For EF Core
    private DeveloperIdentityMapping() { }

    internal static DeveloperIdentityMapping Create(
        Guid organizationId,
        Guid employeeProfileId,
        string sourceSystem,
        string externalAccountId,
        string externalDisplayName)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization id is required.", nameof(organizationId));
        if (employeeProfileId == Guid.Empty)
            throw new ArgumentException("Employee profile id is required.", nameof(employeeProfileId));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException("Source system is required.", nameof(sourceSystem));
        if (string.IsNullOrWhiteSpace(externalAccountId))
            throw new ArgumentException("External account id is required.", nameof(externalAccountId));
        if (string.IsNullOrWhiteSpace(externalDisplayName))
            throw new ArgumentException("External display name is required.", nameof(externalDisplayName));

        return new DeveloperIdentityMapping
        {
            OrganizationId = organizationId,
            EmployeeProfileId = employeeProfileId,
            SourceSystem = sourceSystem,
            ExternalAccountId = externalAccountId,
            ExternalDisplayName = externalDisplayName
        };
    }

    internal void UpdateDisplayName(string externalDisplayName)
    {
        if (string.IsNullOrWhiteSpace(externalDisplayName))
            throw new ArgumentException("External display name is required.", nameof(externalDisplayName));

        ExternalDisplayName = externalDisplayName;
        MarkUpdated();
    }
}
